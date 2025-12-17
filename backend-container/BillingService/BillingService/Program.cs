using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using System.Text;
using System.Security.Claims;
using BillingService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
LogConnectionInfo(connectionString, connectionSource);

builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services
builder.Services.AddScoped<BillingService.Services.IRevenueMetricsService, BillingService.Services.RevenueMetricsService>();

if (useAzureDefaultCredential)
{
    builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
}

builder.Services.AddDbContext<BillingDbContext>((sp, options) =>
{
    if (useAzureDefaultCredential)
    {
        var sqlConn = sp.GetRequiredService<SqlConnection>();
        options.UseSqlServer(sqlConn, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing");
            sql.MigrationsAssembly(typeof(BillingDbContext).Assembly.GetName().Name);
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing");
            sql.MigrationsAssembly(typeof(BillingDbContext).Assembly.GetName().Name);
        });
    }
});

var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["SecretKey"] ?? "dev_secret_key_change";
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

builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing Service API", Version = "v1" });
});

builder.Services.AddHealthChecks().AddDbContextCheck<BillingDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
if (!app.Environment.IsProduction() && string.Equals(app.Configuration["BILLING_BYPASS_AUTH"], "true", StringComparison.OrdinalIgnoreCase))
{
    app.Use(async (ctx, next) =>
    {
        if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                new Claim(ClaimTypes.Name, "BillingService Dev User")
            };
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "BypassAuth"));
        }
        await next();
    });
}
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsProduction())
{
    await ApplyMigrationsAsync(app.Services);
}

app.MapDefaultEndpoints();

await app.RunAsync();

static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__BillingDb"])) { cs = config["ConnectionStrings__BillingDb"]; src = "ConnectionStrings__BillingDb env var"; }
    else { cs = config.GetConnectionString("BillingDb"); src = "appsettings"; }
    if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("No SQL connection string configured.");
    var useAzure = string.Equals(config[UseAzureDefaultCredentialKey], "true", StringComparison.OrdinalIgnoreCase);
    var csb = new SqlConnectionStringBuilder(cs);
    if (useAzure)
    {
        void R(string k){ if (csb.ContainsKey(k)){ csb.Remove(k); } }
        R("User ID"); R("User"); R("UID"); R("Password"); R("Pwd"); R("Authentication");
    }
    return (csb.ConnectionString, src!, useAzure);
}

static void LogConnectionInfo(string conn, string source)
{
    try
    {
        var csb = new SqlConnectionStringBuilder(conn);
        var auth = csb.ContainsKey("Authentication") ? csb["Authentication"] : "(none)";
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

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Billing)...");
        await db.Database.MigrateAsync();
        Console.WriteLine("[Startup] Billing migrations complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Billing migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}
