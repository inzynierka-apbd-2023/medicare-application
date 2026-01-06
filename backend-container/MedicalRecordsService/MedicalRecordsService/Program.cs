using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using MedicalRecordsService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;



// Constants (Must be top-level for static local functions to see them if they are const)
const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    // Register MediatR for CQRS pattern
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));



    var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                         ?? builder.Configuration.GetConnectionString("MedicareDb") 
                         ?? builder.Configuration.GetConnectionString("MedicalRecordsDb") 
                         ?? throw new InvalidOperationException("No SQL connection string configured.");

    LogConnectionInfo(connectionString, "Config");



    builder.Services.AddControllers();

    builder.Services.AddDbContext<MedicalRecordsDbContext>((sp, options) =>
    {
        // Suppress EF Core 9 PendingModelChangesWarning for local development
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "medical");
            sql.MigrationsAssembly(typeof(MedicalRecordsDbContext).Assembly.GetName().Name);
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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Medical Records Service API", Version = "v1", Description = "Medical Records domain API" });
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

    builder.Services.AddHealthChecks().AddDbContextCheck<MedicalRecordsDbContext>();

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
    var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Medical Records)...");
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
        Console.WriteLine($"[Startup] Medical Records migrations in assembly: {string.Join(",", all)}");
        await db.Database.MigrateAsync();
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[Startup] Medical Records applied migrations: {string.Join(",", applied)} (history: medical.__EFMigrationsHistory)");
        var pendingAfter = all.Except(applied);
        Console.WriteLine($"[Startup] Medical Records pending AFTER apply: {string.Join(",", pendingAfter)}");
        await SeedCatalogAsync(db);
        Console.WriteLine("[Startup] Medical Records migrations & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Medical Records migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task SeedCatalogAsync(MedicalRecordsDbContext db)
{
    await MedicalRecordsService.Data.MockDataSeeder.SeedAsync(db);
}
