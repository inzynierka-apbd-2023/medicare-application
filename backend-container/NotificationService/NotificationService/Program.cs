using Microsoft.EntityFrameworkCore;
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

const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

builder.Services.AddControllers();

var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
LogConnectionInfo(connectionString, connectionSource);

if (useAzureDefaultCredential)
{
    builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
}

builder.Services.AddDbContext<NotificationsDbContext>((sp, options) =>
{
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

builder.Services.AddHealthChecks().AddDbContextCheck<NotificationsDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsProduction())
{
    await ApplyMigrationsAsync(app.Services);
}

// MQ bootstrap (minimal; will evolve). Starts a consumer that writes inbound notifications to DB.
_ = Task.Run(() => StartRabbitConsumer(app.Services));

await app.RunAsync();

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Notifications)...");
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
        Console.WriteLine($"[Startup] Notifications migrations in assembly: {string.Join(",", all)}");
        await db.Database.MigrateAsync();
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[Startup] Notifications applied migrations: {string.Join(",", applied)} (history: notifications.__EFMigrationsHistory)");
        var pendingAfter = all.Except(applied);
        Console.WriteLine($"[Startup] Notifications pending AFTER apply: {string.Join(",", pendingAfter)}");
        Console.WriteLine("[Startup] Notifications migrations complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Notifications migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__DefaultConnection"])) { cs = config["ConnectionStrings__DefaultConnection"]; src = "ConnectionStrings__DefaultConnection env var"; }
    else { cs = config.GetConnectionString("DefaultConnection"); src = "appsettings"; }
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

static void StartRabbitConsumer(IServiceProvider services)
{
    try
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "rabbitmq";
        var user = Environment.GetEnvironmentVariable("RABBITMQ__USERNAME") ?? "guest";
        var pass = Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD") ?? "guest";
        var factory = new ConnectionFactory { HostName = host, UserName = user, Password = pass, DispatchConsumersAsync = true };
        var conn = factory.CreateConnection();
        var channel = conn.CreateModel();
        var queue = "notifications.events";
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<NotificationEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (evt != null)
                {
                    using var scope = services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
                    db.Notifications.Add(new NotificationService.Models.Notification
                    {
                        Recipient_User_Id = evt.RecipientUserId,
                        Description = evt.Description,
                        Type = evt.Type,
                        Source_Service = evt.SourceService,
                        Action_Url = evt.ActionUrl,
                        Priority_Level = evt.PriorityLevel,
                        Expires_At = evt.ExpiresAt,
                        Creation_Date = DateTime.UtcNow,
                        Is_Read = false
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notifications MQ] Error processing message: {ex.Message}");
            }
            finally
            {
                await Task.CompletedTask;
            }
        };
        channel.BasicConsume(consumer, queue: queue, autoAck: true);
        Console.WriteLine("[Notifications MQ] Consumer started on queue 'notifications.events'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Notifications MQ] Consumer failed to start: {ex.Message}");
    }
}

public record NotificationEvent(
    string RecipientUserId,
    string? Description,
    byte Type,
    string? SourceService,
    string? ActionUrl,
    string? PriorityLevel,
    DateTime? ExpiresAt
);
