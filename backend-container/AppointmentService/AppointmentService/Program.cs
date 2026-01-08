using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using AppointmentService.Data;
using AppointmentService.Services;
using AppointmentService.Features.DoctorSchedule.Services;
using AppointmentService.Features.DoctorDashboard.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string AuthenticationKeyword = "Authentication";

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                     ?? builder.Configuration.GetConnectionString("MedicareDb") 
                     ?? builder.Configuration.GetConnectionString("AppointmentDb") 
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

LogConnectionInfo(connectionString, "Config");

builder.Services.AddControllers();
builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddHttpClient<AppointmentService.Services.IBillingServiceClient, AppointmentService.Services.BillingServiceClient>(client =>
{
    client.BaseAddress = new("http://billingservice"); 
});


// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add notification service
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AppointmentService.Features.Metrics.Services.IAppointmentMetricsService, AppointmentService.Features.Metrics.Services.AppointmentMetricsService>();

// Add doctor schedule service
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();

// Add doctor dashboard service
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();

// Doctor schedule service now uses direct database queries (UserProfiles table)
// instead of HTTP calls - no external service dependencies needed

builder.Services.AddDbContext<AppointmentDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "appointment");
        sql.MigrationsAssembly(typeof(AppointmentDbContext).Assembly.GetName().Name);
    });
});

var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwt["Issuer"] ?? "MedicareApp";
var audience = jwt["Audience"] ?? "MedicareUsers";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            RoleClaimType = "role"
        };
    });

builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (allowed.Length == 0 || allowed.Contains("*"))
        {
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Appointment Service API", Version = "v1", Description = "Appointment domain API" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
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

builder.Services.AddHealthChecks().AddDbContextCheck<AppointmentDbContext>();
// Background services
builder.Services.AddHostedService<OverdueStatusUpdater>();
builder.Services.AddHostedService<UpcomingAppointmentNotifier>();
builder.Services.AddHostedService<DoctorArchivedConsumer>();
builder.Services.AddHostedService<AppointmentPaymentConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsProduction())
{
    await ApplyMigrationsAsync(app.Services);
}

app.MapDefaultEndpoints();

await app.RunAsync();

static void LogConnectionInfo(string conn, string source)
{
    try
    {
        var csb = new SqlConnectionStringBuilder(conn);
        var auth = csb.ContainsKey(AuthenticationKeyword) ? csb[AuthenticationKeyword] : "(none)";
        Console.WriteLine($"[Startup] Using SQL Server connection (source: {source}) -> Server: {csb.DataSource}, Database: {csb.InitialCatalog}, Auth: {auth}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Connection info parse failed: {ex.Message}");
    }
}

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Appointment)...");
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys.ToArray();
        Console.WriteLine($"[Startup] Appointment migrations in assembly: {string.Join(",", all)}");

        // Ensure schema exists before migrations/ensure-created
        const string ensureSchemaSql = "IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'appointment') EXEC('CREATE SCHEMA [appointment]');";
        try
        {
            await db.Database.ExecuteSqlRawAsync(ensureSchemaSql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Warning ensuring schema [appointment]: {ex.Message}");
        }

        if (all.Length > 0)
        {
            await db.Database.MigrateAsync();
            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
            Console.WriteLine($"[Startup] Appointment applied migrations: {string.Join(",", applied)} (history: appointment.__EFMigrationsHistory)");
            var pendingAfter = all.Except(applied);
            Console.WriteLine($"[Startup] Appointment pending AFTER apply: {string.Join(",", pendingAfter)}");
        }
        else
        {
            // No migrations found in assembly. Fall back to EnsureCreated to materialize the model.
            Console.WriteLine("[Startup] No migrations found. Falling back to Database.EnsureCreated for Appointment DB.");
            await db.Database.EnsureCreatedAsync();
        }

        // Ensure performance index for overdue background job exists
        const string ensureOverdueIndexSql = @"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    JOIN sys.schemas s ON o.schema_id = s.schema_id
    WHERE i.name = 'IX_Appointment_Status_ScheduledEndAt'
      AND o.name = 'Appointment'
      AND s.name = 'appointment'
)
BEGIN
    CREATE INDEX IX_Appointment_Status_ScheduledEndAt
    ON [appointment].[Appointment] ([Status], [ScheduledEndAt]);
END
";
        try
        {
            await db.Database.ExecuteSqlRawAsync(ensureOverdueIndexSql);
            Console.WriteLine("[Startup] Ensured index IX_Appointment_Status_ScheduledEndAt on [appointment].[Appointment].");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Warning ensuring overdue index: {ex.Message}");
        }

        await SeedCatalogAsync(db); // Enabled to provision mock data
        Console.WriteLine("[Startup] Appointment DB initialization complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Appointment migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task SeedCatalogAsync(AppointmentDbContext db)
{
    await AppointmentService.Data.MockDataSeeder.SeedAsync(db);
}

// Make Program class public for testing
public partial class Program { }
