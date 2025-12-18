using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using PatientService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PatientService.Infrastructure.Messaging;
using MediatR;


// Constants (Must be top-level for static local functions to see them if they are const)
const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    // Normalize connection early
    var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
    LogConnectionInfo(connectionString, connectionSource);

    builder.Services.AddControllers();
    builder.Services.AddMediatR(typeof(Program).Assembly);

    if (useAzureDefaultCredential)
    {
        builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
    }

    builder.Services.AddDbContext<PatientDbContext>((sp, options) =>
    {
        // Suppress EF Core 9 PendingModelChangesWarning for local development
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        
        if (useAzureDefaultCredential)
        {
            var sqlConn = sp.GetRequiredService<SqlConnection>();
            options.UseSqlServer(sqlConn, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "patient");
                sql.MigrationsAssembly(typeof(PatientDbContext).Assembly.GetName().Name);
            });
        }
        else
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "patient");
                sql.MigrationsAssembly(typeof(PatientDbContext).Assembly.GetName().Name);
            });
        }
    });

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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Patient Service API", Version = "v1", Description = "Patient domain API" });
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

    builder.Services.AddHealthChecks().AddDbContextCheck<PatientDbContext>();
    builder.AddRabbitMQClient("rabbitmq");
    builder.Services.AddHostedService<UserRegisteredConsumer>();
    builder.Services.AddScoped<PatientService.Features.Metrics.Services.IPatientMetricsService, PatientService.Features.Metrics.Services.PatientMetricsService>();

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
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Application terminated unexpectedly: {ex}");
    await Task.Delay(10000); // Wait 10s to ensure logs are flushed/read
    throw;
}

static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__PatientServiceDb"])) { cs = config["ConnectionStrings__PatientServiceDb"]; src = "ConnectionStrings__PatientServiceDb env var"; }
    else { cs = config.GetConnectionString("PatientServiceDb"); src = "appsettings"; }
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

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var servicesProvider = scope.ServiceProvider;
    var db = servicesProvider.GetRequiredService<PatientDbContext>();
    
    int retries = 0;
    while (true)
    {
        try
        {
            Console.WriteLine("[Startup] Applying EF Core migrations (Patient)...");
            var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
            Console.WriteLine($"[Startup] Patient migrations in assembly: {string.Join(",", all)}");
            await db.Database.MigrateAsync();
            var applied = await db.Database.GetAppliedMigrationsAsync();
            Console.WriteLine($"[Startup] Patient applied migrations: {string.Join(",", applied)} (history: patient.__EFMigrationsHistory)");
            var pendingAfter = all.Except(applied);
            Console.WriteLine($"[Startup] Patient pending AFTER apply: {string.Join(",", pendingAfter)}");
            await SeedCatalogAsync(db);
            Console.WriteLine("[Startup] Patient migrations & seeding complete.");
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($"[Startup] Patient migration failed: {ex.Message}. Retry {retries}...");
            if (retries > 10) throw;
            await Task.Delay(5000);
        }
    }
}

static async Task SeedCatalogAsync(PatientDbContext db)
{
    // Minimal seed: none for now; left for future idempotent inserts if needed
    await Task.CompletedTask;
}
