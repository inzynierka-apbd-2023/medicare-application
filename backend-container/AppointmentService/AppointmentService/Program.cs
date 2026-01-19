using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Services;
using AppointmentService.Services.Messaging.Consumers;
using AppointmentService.Services.Messaging.Notifiers;
using AppointmentService.Features.DoctorSchedule.Services;
using AppointmentService.Features.DoctorDashboard.Services;
using AppointmentService.Data.Seeders;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                        ?? builder.Configuration.GetConnectionString("MedicareDb") 
                        ?? builder.Configuration.GetConnectionString("AppointmentDb") 
                        ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppointmentDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "appointment");
        sql.MigrationsAssembly(typeof(AppointmentDbContext).Assembly.GetName().Name);
    });
});

builder.AddMedicareAuthentication();

builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowed == null || allowed.Length == 0)
        {
            throw new InvalidOperationException("CORS AllowedOrigins must be configured in appsettings.");
        }
        p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.AddMedicareMassTransit<AppointmentDbContext>(x =>
{
    x.AddConsumer<AppointmentPaymentConsumer>();
    x.AddConsumer<DoctorArchivedConsumer>();
    x.AddRequestClient<Medicare.Messaging.Contracts.IGetPatient>();
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AppointmentService.Features.Metrics.Services.IAppointmentMetricsService, AppointmentService.Features.Metrics.Services.AppointmentMetricsService>();
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();

builder.Services.AddHostedService<OverdueStatusUpdater>();
builder.Services.AddHostedService<UpcomingAppointmentNotifier>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Medicare Appointment Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            }, Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<AppointmentDbContext>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseGlobalExceptionHandling();

await DbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseCors("DefaultPolicy");
app.UseDefaultRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program { }
