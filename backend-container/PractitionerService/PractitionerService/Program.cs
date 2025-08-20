using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using PractitionerService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";

var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
LogConnectionInfo(connectionString, connectionSource);

builder.Services.AddControllers();

builder.Services.AddMediatR(typeof(Program).Assembly);

// Register HttpClient for UserService communication
builder.Services.AddHttpClient<PractitionerService.Services.IStaffService, PractitionerService.Services.StaffService>(client =>
{
    // Use docker-compose service DNS by default; override via Services:UserService:BaseUrl when needed.
    var userServiceUrl = builder.Configuration["Services:UserService:BaseUrl"] ?? "http://user-service:8080";
    client.BaseAddress = new Uri(userServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Named client used by controllers for direct calls to UserService
builder.Services.AddHttpClient("UserService", client =>
{
    var userServiceUrl = builder.Configuration["Services:UserService:BaseUrl"] ?? "http://user-service:8080";
    client.BaseAddress = new Uri(userServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Remove duplicate named client registration

// Register Staff Service
builder.Services.AddScoped<PractitionerService.Services.IStaffService, PractitionerService.Services.StaffService>();

if (useAzureDefaultCredential)
{
    builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
}

builder.Services.AddDbContext<PractitionerDbContext>((sp, options) =>
{
    if (useAzureDefaultCredential)
    {
        var sqlConn = sp.GetRequiredService<SqlConnection>();
        options.UseSqlServer(sqlConn, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "practitioner");
            sql.MigrationsAssembly(typeof(PractitionerDbContext).Assembly.GetName().Name);
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", "practitioner");
            sql.MigrationsAssembly(typeof(PractitionerDbContext).Assembly.GetName().Name);
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Practitioner Service API", Version = "v1", Description = "Practitioner domain API" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<PractitionerDbContext>();

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

await app.RunAsync();

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

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PractitionerDbContext>();
    try
    {
    Console.WriteLine("[Startup] Applying EF Core migrations (Practitioner)...");
    var all = db.GetService<IMigrationsAssembly>().Migrations.Keys;
    Console.WriteLine($"[Startup] Practitioner migrations in assembly: {string.Join(",", all)}");
    await db.Database.MigrateAsync();
    var applied = await db.Database.GetAppliedMigrationsAsync();
    Console.WriteLine($"[Startup] Practitioner applied migrations: {string.Join(",", applied)} (history: practitioner.__EFMigrationsHistory)");
    // pending AFTER apply can be derived by set difference
    var pendingAfter = all.Except(applied);
    Console.WriteLine($"[Startup] Practitioner pending AFTER apply: {string.Join(",", pendingAfter)}");
        await SeedCatalogAsync(db);
    await SeedTestDataAsync(db);
        Console.WriteLine("[Startup] Practitioner migrations & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Practitioner migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task SeedCatalogAsync(PractitionerDbContext db)
{
    var anyServices = await db.Services.AnyAsync();
    if (!anyServices)
    {
        db.Services.AddRange(
            new PractitionerService.Models.MedicalService { Name = "General Consultation", Description = "Routine check and consultation" },
            new PractitionerService.Models.MedicalService { Name = "Cardiology", Description = "Heart-related services" },
            new PractitionerService.Models.MedicalService { Name = "Dermatology", Description = "Skin-related services" }
        );
    }
    var anySpecs = await db.Specializations.AnyAsync();
    if (!anySpecs)
    {
        db.Specializations.AddRange(
            new PractitionerService.Models.Specialization { Name = "Cardiologist" },
            new PractitionerService.Models.Specialization { Name = "Dermatologist" },
            new PractitionerService.Models.Specialization { Name = "General Practitioner" }
        );
    }
    if (!anyServices || !anySpecs)
    {
        await db.SaveChangesAsync();
        Console.WriteLine("[Startup] Seeded default practitioner catalog (services & specializations).");
    }
}

static async Task SeedTestDataAsync(PractitionerDbContext db)
{
    // Execute idempotent SQL to insert rich sample data across all tables
    var sql = @"-- Services (idempotent by name)
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='General Consultation')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('General Consultation','Routine check and consultation');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Cardiology Review')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Cardiology Review','Heart health assessment');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Dermatology Check')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Dermatology Check','Skin examination');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Pediatric Visit')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Pediatric Visit','Child health appointment');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Orthopedic Assessment')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Orthopedic Assessment','Bone and joint evaluation');

-- Specializations
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='General Practitioner')
    INSERT INTO practitioner.Specialization (Name) VALUES ('General Practitioner');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Cardiologist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Cardiologist');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Dermatologist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Dermatologist');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Pediatrician')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Pediatrician');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Orthopedist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Orthopedist');

-- Doctors (two sample doctors referencing existing user IDs if available)
IF NOT EXISTS (SELECT 1 FROM practitioner.Doctor)
BEGIN
    DECLARE @u1 nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] ORDER BY Created_At);
    IF @u1 IS NULL SET @u1 = CONVERT(varchar(36),NEWID());
    DECLARE @u2 nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] WHERE Id <> @u1 ORDER BY Created_At);
    IF @u2 IS NULL SET @u2 = CONVERT(varchar(36),NEWID());

    INSERT INTO practitioner.Doctor (UserId, Bio) VALUES
        (@u1, 'Experienced general practitioner with a focus on preventative care'),
        (@u2, 'Cardiology specialist with 10 years of clinical experience');

    -- Link doctor specializations
    DECLARE @gpId nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='General Practitioner');
    DECLARE @cardId nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Cardiologist');
    DECLARE @d1 nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Doctor ORDER BY CreatedAt);
    DECLARE @d2 nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE Id <> @d1 ORDER BY CreatedAt);
    IF @d1 IS NOT NULL AND @gpId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Specialization WHERE DoctorId=@d1 AND SpecializationId=@gpId)
        INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@d1, @gpId);
    IF @d2 IS NOT NULL AND @cardId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Specialization WHERE DoctorId=@d2 AND SpecializationId=@cardId)
        INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@d2, @cardId);

    -- Schedules
    IF @d1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Schedule WHERE DoctorId=@d1)
        INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES
            (@d1,1,'09:00','12:00'),
            (@d1,3,'13:00','17:00');
    IF @d2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Schedule WHERE DoctorId=@d2)
        INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES
            (@d2,2,'10:00','14:00');
END

-- Receptionist (sample)
IF NOT EXISTS (SELECT 1 FROM practitioner.Receptionist)
BEGIN
    DECLARE @rUser nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] WHERE Id NOT IN (SELECT UserId FROM practitioner.Doctor) ORDER BY Created_At);
    IF @rUser IS NULL SET @rUser = CONVERT(varchar(36),NEWID());
    INSERT INTO practitioner.Receptionist (UserId) VALUES (@rUser);
END

-- Refresh view (if present)
IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';";
    try
    {
        await db.Database.ExecuteSqlRawAsync(sql);
        Console.WriteLine("[Startup] Seeded practitioner test data.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Test data seed warning: {ex.Message}");
    }
}

// Make Program class accessible for testing
public static partial class Program { }
