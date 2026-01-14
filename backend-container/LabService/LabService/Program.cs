using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using LabService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string AuthenticationKeyword = "Authentication";

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                     ?? builder.Configuration.GetConnectionString("MedicareDb") 
                     ?? builder.Configuration.GetConnectionString("LabDb") 
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

LogConnectionInfo(connectionString, "Config");

builder.Services.AddControllers();

// Register MediatR for CQRS pattern
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddDbContext<LabDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "lab");
        sql.MigrationsAssembly(typeof(LabDbContext).Assembly.GetName().Name);
    });
});

// Auth (JWT)
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Lab Service API", Version = "v1", Description = "Lab domain API" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<LabDbContext>();

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

// Always apply migrations on startup (including production)
await ApplyMigrationsAsync(app.Services);

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
    var db = scope.ServiceProvider.GetRequiredService<LabDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Lab)...");
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
        Console.WriteLine($"[Startup] Lab migrations in assembly: {string.Join(",", all)}");
        await db.Database.MigrateAsync();
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[Startup] Lab applied migrations: {string.Join(",", applied)} (history: lab.__EFMigrationsHistory)");
        var pendingAfter = all.Except(applied);
        Console.WriteLine($"[Startup] Lab pending AFTER apply: {string.Join(",", pendingAfter)}");
        await SeedCatalogAsync(db);
        Console.WriteLine("[Startup] Lab migrations & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Lab migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task SeedCatalogAsync(LabDbContext db)
{
    await LabService.Data.MockDataSeeder.SeedAsync(db);
}
