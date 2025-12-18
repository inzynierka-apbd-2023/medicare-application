using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

builder.Services.AddControllers();
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddHostedService<NotificationService.Services.NotificationConsumerService>();

var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
LogConnectionInfo(connectionString, connectionSource);

if (useAzureDefaultCredential)
{
    builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
}

builder.Services.AddDbContext<NotificationsDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    if (useAzureDefaultCredential)
    {
        var sqlConn = sp.GetRequiredService<SqlConnection>();
        options.UseSqlServer(sqlConn, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
            sql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.GetName().Name);
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
            sql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.GetName().Name);
        });
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1", Description = "User notifications" });
});

// JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwt["Issuer"] ?? "MedicareApp";
var audience = jwt["Audience"] ?? "MedicareUsers";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddHealthChecks().AddDbContextCheck<NotificationsDbContext>();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapDefaultEndpoints();

// Always apply migrations on startup to ensure schema exists in all environments
// Optional one-time purge remains gated by env var
{
    var purgeRequested = string.Equals(app.Configuration["PURGE_NOTIFICATIONS_SCHEMA"], "true", StringComparison.OrdinalIgnoreCase);
    if (purgeRequested)
    {
        await PurgeNotificationsSchemaAsync(app.Services);
    }
    await ApplyMigrationsAsync(app.Services);
}

// Cleanup loop
_ = Task.Run(() => StartCleanupLoop(app.Services));

await app.RunAsync();

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Notifications)...");

        // Self-heal: if migration history exists but the Notification table doesn't, drop history so migrations can recreate.
        await db.Database.OpenConnectionAsync();
        var conn = db.Database.GetDbConnection();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT OBJECT_ID(N'[notifications].[__EFMigrationsHistory]')";
            var histObj = await cmd.ExecuteScalarAsync();
            cmd.CommandText = "SELECT OBJECT_ID(N'[notifications].[Notification]')";
            var tableObj = await cmd.ExecuteScalarAsync();
            bool historyExists = histObj != null && histObj != DBNull.Value && Convert.ToInt32(histObj) != 0;
            bool tableExists = tableObj != null && tableObj != DBNull.Value && Convert.ToInt32(tableObj) != 0;
            if (historyExists && !tableExists)
            {
                Console.WriteLine("[Startup] Detected history without table. Dropping notifications.__EFMigrationsHistory to reapply migrations...");
                await db.Database.ExecuteSqlRawAsync("DROP TABLE [notifications].[__EFMigrationsHistory]");
            }
        }

        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
        Console.WriteLine($"[Startup] Notifications migrations in assembly: {string.Join(",", all)}");
        await db.Database.MigrateAsync();
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[Startup] Notifications applied migrations: {string.Join(",", applied)} (history: notifications.__EFMigrationsHistory)");
        var pendingAfter = all.Except(applied);
        Console.WriteLine($"[Startup] Notifications pending AFTER apply: {string.Join(",", pendingAfter)}");

        // Verify table exists after migration
        using (var verifyCmd = db.Database.GetDbConnection().CreateCommand())
        {
            verifyCmd.CommandText = "SELECT OBJECT_ID(N'[notifications].[Notification]')";
            var tableObj = await verifyCmd.ExecuteScalarAsync();
            bool tableExists = tableObj != null && tableObj != DBNull.Value && Convert.ToInt32(tableObj) != 0;
            if (!tableExists)
            {
                Console.WriteLine("[Startup] ERROR: notifications.Notification table still missing after migrations.");
            }
        }

        Console.WriteLine("[Startup] Notifications migrations complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Notifications migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task PurgeNotificationsSchemaAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    try
    {
        Console.WriteLine("[Startup] Purging notifications schema (drop objects + schema), and resetting upcoming flags...");
        var dropSql = @"
IF EXISTS (SELECT 1 FROM sys.objects o JOIN sys.schemas s ON o.schema_id = s.schema_id WHERE s.name = 'notifications' AND o.name = '__EFMigrationsHistory' AND o.type = 'U')
BEGIN
    DROP TABLE [notifications].[__EFMigrationsHistory];
END
IF EXISTS (SELECT 1 FROM sys.objects o JOIN sys.schemas s ON o.schema_id = s.schema_id WHERE s.name = 'notifications' AND o.name = 'Notification' AND o.type = 'U')
BEGIN
    DROP TABLE [notifications].[Notification];
END
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'notifications')
BEGIN
    DROP SCHEMA [notifications];
END
";
        await db.Database.ExecuteSqlRawAsync(dropSql);

        // Reset upcoming notification flag so the appointment service can emit new reminders
        var resetSql = @"
UPDATE a
SET a.UpcomingNotificationSentAt = NULL
FROM [appointment].[Appointment] a
WHERE a.ScheduledAt >= SYSUTCDATETIME()
  AND a.Status IN ('Scheduled','Confirmed');
";
        try
        {
            await db.Database.ExecuteSqlRawAsync(resetSql);
            Console.WriteLine("[Startup] Reset UpcomingNotificationSentAt for future scheduled/confirmed appointments.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Warning resetting upcoming flags: {ex.Message}");
        }
        Console.WriteLine("[Startup] Purge complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Purge failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__NotificationDb"])) { cs = config["ConnectionStrings__NotificationDb"]; src = "ConnectionStrings__NotificationDb env var"; }
    else { cs = config.GetConnectionString("NotificationDb"); src = "appsettings"; }
    if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("No SQL connection string configured.");
    var useAzure = string.Equals(config[UseAzureDefaultCredentialKey], "true", StringComparison.OrdinalIgnoreCase);
    var csb = new SqlConnectionStringBuilder(cs);
    if (useAzure)
    {
        bool modified = false;
        void R(string k){ if (csb.ContainsKey(k)){ csb.Remove(k); modified = true; } }
        R("User ID"); R("User"); R("UID"); R("Password"); R("Pwd"); R(AuthenticationKeyword);
        if (modified) Console.WriteLine("[Startup] Normalized connection string for AAD token (removed credentials / Authentication).");
    }
    return (csb.ConnectionString, src!, useAzure);
}

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

static SqlConnection CreateTokenSqlConnection(string connectionString)
{
    var credential = new DefaultAzureCredential();
    var conn = new SqlConnection(connectionString)
    {
        AccessToken = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })).Token
    };
    return conn;
}



static async Task StartCleanupLoop(IServiceProvider services)
{
    while (true)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            // delete read notifications older than 30 days or any expired ones (only if table exists)
            await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[notifications].[Notification]') IS NOT NULL
BEGIN
    DELETE FROM [notifications].[Notification]
    WHERE (Is_Read = 1 AND Creation_Date < {0}) OR (Expires_At IS NOT NULL AND Expires_At < SYSUTCDATETIME());
END
", cutoff);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Cleanup] Notification cleanup failed: {ex.Message}");
        }
        await Task.Delay(TimeSpan.FromHours(6));
    }
}


