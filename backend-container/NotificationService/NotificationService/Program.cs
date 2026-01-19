using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.Data;
using NotificationService.Data.Seeders;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                        ?? builder.Configuration.GetConnectionString("MedicareDb") 
                        ?? builder.Configuration.GetConnectionString("NotificationDb") 
                        ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();
builder.AddRabbitMQClient("rabbitmq");
builder.AddMedicareMassTransit<NotificationsDbContext>(x =>
{
    x.AddConsumer<NotificationService.Consumers.NotificationCreatedConsumer>();
});

builder.Services.AddHostedService<NotificationService.Services.NotificationConsumerService>();


// Email service configuration
builder.Services.Configure<NotificationService.Services.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<NotificationService.Services.IEmailService, NotificationService.Services.GmailEmailService>();

builder.Services.AddDbContext<NotificationsDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
        sql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.GetName().Name);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1", Description = "User notifications" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<NotificationsDbContext>();

// Cleanup service
builder.Services.AddHostedService<NotificationService.Services.NotificationCleanupService>();

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultPolicy");
app.UseDefaultRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapDefaultEndpoints();

await DbSeeder.SeedAsync(app.Services);

await app.RunAsync();
