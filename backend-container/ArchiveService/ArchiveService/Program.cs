using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Data.Common;
using ArchiveService.Data;
using ArchiveService.Models;
using ArchiveService.Messaging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configure MSSQL Connection
// Configure MSSQL Connection
const string AuthenticationKeyword = "Authentication";
    
    var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                         ?? builder.Configuration.GetConnectionString("MedicareDb") 
                         ?? builder.Configuration.GetConnectionString("ArchiveDb") 
                         ?? throw new InvalidOperationException("No SQL connection string configured.");

    LogConnectionInfo(connectionString, "Config");

builder.Services.AddDbContext<ArchiveDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddHostedService<DoctorArchiveConsumer>();

// Auth (JWT)
var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwt["Issuer"] ?? "MedicareApp";
var audience = jwt["Audience"] ?? "MedicareUsers";
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
    try
    {
        // Standard migration application for MSSQL
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log detailed error if migration fails (e.g. connection issues)
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Minimal controller-style endpoints
app.MapGet("/archive/doctors/{doctorId}", async (Guid doctorId, ArchiveDbContext db) =>
{
    var archived = await db.ArchivedDoctors.FindAsync(doctorId);
    return archived is null ? Results.NotFound() : Results.Ok(archived);
}).RequireAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

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
