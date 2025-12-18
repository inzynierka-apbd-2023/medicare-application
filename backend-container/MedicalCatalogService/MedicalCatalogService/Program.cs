using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Data;
using MedicalCatalogService.Data;


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    var connectionString = builder.Configuration.GetConnectionString("MedicalCatalogDb") ?? "";
    var useAzureDefaultCredential = (builder.Configuration["USE_AZURE_DEFAULT_CREDENTIAL"] ?? builder.Configuration["UseAzureDefaultCredential"])?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

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
            o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey))
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
                if (useAzureDefaultCredential)
                {
                   // Azure logic handled by DbContext configuration if needed, 
                   // but for now keeping it simple as per other services or just logging.
                   // The previous placeholder code was removed.
                }

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
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Application terminated unexpectedly: {ex}");
    await Task.Delay(10000); // Wait 10s to ensure logs are flushed/read
    throw;
}
