using Azure.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Text;
using MedicalCatalogService.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    const string AuthenticationKeyword = "Authentication";

    var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                         ?? builder.Configuration.GetConnectionString("MedicareDb") 
                         ?? builder.Configuration.GetConnectionString("MedicalCatalogDb") 
                         ?? throw new InvalidOperationException("No SQL connection string configured.");

    LogConnectionInfo(connectionString, "Config");

    builder.Services.AddDbContext<MedicalCatalogDbContext>((sp, options) =>
    {
        // Suppress EF Core 9 PendingModelChangesWarning for local development
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        
        options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
            sql.MigrationsAssembly(typeof(MedicalCatalogDbContext).Assembly.GetName().Name);
            sql.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        });
    });

    builder.Services.AddHealthChecks().AddDbContextCheck<MedicalCatalogDbContext>();
    builder.Services.AddControllers();

    // JWT Authentication with proper token validation
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
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("CatalogImport", policy =>
        {
            // In Development, allow anonymous for import endpoints; otherwise require auth
            if ((builder.Environment?.IsDevelopment() ?? false) || (builder.Environment?.EnvironmentName == "Test"))
            {
                policy.RequireAssertion(_ => true);
            }
            else
            {
                policy.RequireAuthenticatedUser();
            }
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "MedicalCatalog API", Version = "v1" });
    });

    builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");

    app.MapControllers();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<MedicalCatalogDbContext>();
        
        // Robust migration loop
        int retries = 0;
        while (true)
        {
            try
            {


                logger.LogInformation("Applying migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
                break; 
            }
            catch (Exception ex)
            {
                retries++;
                logger.LogError(ex, "An error occurred while migrating the database. Retry {RetryCount}...", retries);
                if (retries > 6) throw; // Max ~30s (6 * 5s)
                await Task.Delay(5000);
            }
        }
    }

    app.MapDefaultEndpoints();

    app.Run();

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
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Application terminated unexpectedly: {ex}");
    await Task.Delay(10000); // Wait 10s to ensure logs are flushed/read
    throw;
}
