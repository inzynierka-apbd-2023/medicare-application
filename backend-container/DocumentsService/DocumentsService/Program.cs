using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using DocumentsService.Data;
using DocumentsService.Infrastructure.Events;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
LogConnectionInfo(connectionString, connectionSource);

builder.Services.AddControllers();
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

if (useAzureDefaultCredential)
{
    builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
}

builder.Services.AddDbContext<DocumentsDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    if (useAzureDefaultCredential)
    {
        var sqlConn = sp.GetRequiredService<SqlConnection>();
        options.UseSqlServer(sqlConn, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            sql.MigrationsAssembly(typeof(DocumentsDbContext).Assembly.GetName().Name);
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            sql.MigrationsAssembly(typeof(DocumentsDbContext).Assembly.GetName().Name);
        });
    }
});

var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["SecretKey"] ?? "dev_secret_key_change"; // keep optional for local
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Documents Service API", Version = "v1", Description = "Clinical documents API" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<DocumentsDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
// Optional: in non-production, allow bypassing auth for smoke tests if env DOCUMENTS_BYPASS_AUTH=true
if (!app.Environment.IsProduction() &&
    string.Equals(app.Configuration["DOCUMENTS_BYPASS_AUTH"], "true", StringComparison.OrdinalIgnoreCase))
{
    app.Use(async (ctx, next) =>
    {
        if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                new Claim(ClaimTypes.Name, "DocumentsService Dev User")
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
    await SeedTestDataAsync(app.Services);
}

app.MapDefaultEndpoints();

await app.RunAsync();

static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__DocumentsDb"])) { cs = config["ConnectionStrings__DocumentsDb"]; src = "ConnectionStrings__DocumentsDb env var"; }
    else { cs = config.GetConnectionString("DocumentsDb"); src = "appsettings"; }
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
    else
    {
        // For local SQL containers, trust the self-signed certificate
        if (!csb.TrustServerCertificate)
        {
            csb.TrustServerCertificate = true;
            Console.WriteLine("[Startup] Enforcing TrustServerCertificate=True for non-Azure connection.");
        }
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
    var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Documents)...");
        var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
        Console.WriteLine($"[Startup] Documents migrations in assembly: {string.Join(",", all)}");
        await db.Database.MigrateAsync();
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[Startup] Documents applied migrations: {string.Join(",", applied)} (history: documents.__EFMigrationsHistory)");
        var pendingAfter = all.Except(applied);
        Console.WriteLine($"[Startup] Documents pending AFTER apply: {string.Join(",", pendingAfter)}");
        Console.WriteLine("[Startup] Documents migrations complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Documents migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task SeedTestDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    try
    {
        // Seed Document Types already done by migration; insert a couple of sample documents if none exist
        if (!await db.Documents.AnyAsync())
        {
            var visitType = await db.DocumentTypes.FirstAsync(t => t.Code == "VISIT_NOTE");
            var rxType = await db.DocumentTypes.FirstAsync(t => t.Code == "PRESCRIPTION");
            var labType = await db.DocumentTypes.FirstAsync(t => t.Code == "LAB_RESULTS");

            var patientId = Guid.NewGuid().ToString();
            var doctorId = Guid.NewGuid().ToString();

            var visit = new DocumentsService.Models.Document
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DocumentTypeId = visitType.Id,
                Type = (int)DocumentsService.Models.DocumentKind.VisitNote,
                Notes = "Initial consultation",
            };
            db.Documents.Add(visit);
            await db.SaveChangesAsync();
            db.VisitDocuments.Add(new DocumentsService.Models.VisitDocument
            {
                DocumentId = visit.Id,
                Symptoms = "Headache, fatigue",
                Findings = "BP 130/85",
                Diagnosis = "Tension headache",
                Recommendations = "Hydration, rest",
                TreatmentPlan = "OTC analgesic",
            });

            var rx = new DocumentsService.Models.Document
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DocumentTypeId = rxType.Id,
                Type = (int)DocumentsService.Models.DocumentKind.Prescription,
                Notes = "Analgesic prescription",
            };
            db.Documents.Add(rx);
            await db.SaveChangesAsync();
            db.Prescriptions.Add(new DocumentsService.Models.Prescription
            {
                DocumentId = rx.Id,
                Medication = "Ibuprofen",
                Dosage = "200mg",
                Frequency = "2x daily",
                DurationDays = 5,
                Instructions = "After meals"
            });

            var lab = new DocumentsService.Models.Document
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DocumentTypeId = labType.Id,
                Type = (int)DocumentsService.Models.DocumentKind.LabResults,
                Notes = "Basic panel",
            };
            db.Documents.Add(lab);
            await db.SaveChangesAsync();
            db.LabResults.Add(new DocumentsService.Models.LabResults
            {
                DocumentId = lab.Id,
                TestType = "CBC",
                TestDate = DateTime.UtcNow.Date.AddDays(-1),
                Laboratory = "Local Lab",
                OverallStatus = "Final",
                Interpretation = "All within normal ranges"
            });
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Documents seed warning: {ex.Message}");
    }
}
