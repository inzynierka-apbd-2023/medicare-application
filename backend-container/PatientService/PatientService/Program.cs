using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PatientService.Data;
using PatientService.Data.Seeders;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"]
                     ?? builder.Configuration.GetConnectionString("MedicareDb")
                     ?? builder.Configuration.GetConnectionString("PatientServiceDb")
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddDbContext<PatientDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "patient");
        sql.MigrationsAssembly(typeof(PatientDbContext).Assembly.GetName().Name);
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Patient Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }, Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<PatientDbContext>();

builder.AddMedicareMassTransit<PatientDbContext>(x =>
{
    x.AddConsumer<PatientService.Messaging.Consumers.PatientDetailsConsumer>();
    x.AddConsumer<PatientService.Messaging.Consumers.UserRegisteredConsumer>();
});
builder.Services.AddScoped<PatientService.Features.Metrics.Services.IPatientMetricsService, PatientService.Features.Metrics.Services.PatientMetricsService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseGlobalExceptionHandling();

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

await DbSeeder.SeedAsync(app.Services);

app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program { }
