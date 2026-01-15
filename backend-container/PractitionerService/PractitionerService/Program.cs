using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using PractitionerService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string AuthenticationKeyword = "Authentication";

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                     ?? builder.Configuration.GetConnectionString("MedicareDb") 
                     ?? builder.Configuration.GetConnectionString("PractitionerServiceDb") 
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

LogConnectionInfo(connectionString, "Config");

builder.Services.AddControllers();
builder.AddRabbitMQClient("rabbitmq");

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
// Note: IStaffService is already registered as a Typed Client via AddHttpClient above
// builder.Services.AddScoped<PractitionerService.Services.IStaffService, PractitionerService.Services.StaffService>();

builder.Services.AddDbContext<PractitionerDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "practitioner");
        sql.MigrationsAssembly(typeof(PractitionerDbContext).Assembly.GetName().Name);
    });
});

builder.Services.AddHostedService<PractitionerService.Services.AppointmentEventListener>();
builder.Services.AddHostedService<PractitionerService.Services.DoctorProfileRequestHandler>();

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
    await PractitionerService.Data.MockDataSeeder.SeedAsync(db);
    // await SeedTestDataAsync(db); // Reverted to use MockDataSeeder as per user request
    await CreateViewsAsync(db);
        Console.WriteLine("[Startup] Practitioner migrations & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Practitioner migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task CreateViewsAsync(PractitionerDbContext db)
{
    // Wait for User_Profile table to exist (UserService creates it)
    const int maxRetries = 10;
    const int retryDelayMs = 1000;
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            // Check if User_Profile table exists
            var checkResult = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN OBJECT_ID('[user].[User_Profile]', 'U') IS NOT NULL THEN 1 ELSE 0 END AS Value"
            ).FirstOrDefaultAsync();

            if (checkResult == 1)
            {
                // User_Profile exists, create the view with join
                var viewSql = @"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
SELECT d.Id AS DoctorId,
       d.UserId,
       up.FirstName,
       up.LastName,
       up.Email,
       up.Phone,
       STUFF((
           SELECT ',' + CAST(ds.SpecializationId AS NVARCHAR(36))
           FROM practitioner.Doctor_Specialization ds
           WHERE ds.DoctorId = d.Id
           FOR XML PATH(''), TYPE
       ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
       NULL AS Services,
       d.IsActive
FROM practitioner.Doctor d
LEFT JOIN [user].[User_Profile] up ON up.User_Id = d.UserId;
";
                await db.Database.ExecuteSqlRawAsync(viewSql);
                Console.WriteLine("[Startup] DoctorDirectory view created with User_Profile join.");
                return;
            }
            
            Console.WriteLine($"[Startup] User_Profile table not ready, waiting... (attempt {attempt}/{maxRetries})");
            await Task.Delay(retryDelayMs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] View creation attempt {attempt} failed: {ex.Message}");
            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelayMs);
            }
        }
    }

    // Fallback: create view without User_Profile join (frontend will fetch names from UserService)
    Console.WriteLine("[Startup] Creating fallback DoctorDirectory view without User_Profile join.");
    try
    {
        var fallbackSql = @"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
SELECT d.Id AS DoctorId,
       d.UserId,
       NULL AS FirstName,
       NULL AS LastName,
       NULL AS Email,
       NULL AS Phone,
       STUFF((
           SELECT ',' + CAST(ds.SpecializationId AS NVARCHAR(36))
           FROM practitioner.Doctor_Specialization ds
           WHERE ds.DoctorId = d.Id
           FOR XML PATH(''), TYPE
       ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
       NULL AS Services,
       d.IsActive
FROM practitioner.Doctor d;
";
        await db.Database.ExecuteSqlRawAsync(fallbackSql);
        Console.WriteLine("[Startup] DoctorDirectory fallback view created. Doctor names will be fetched via UserService.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Fallback view creation also failed: {ex.Message}");
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

-- Specialization Services (Link)
-- General Practitioner -> General Consultation
IF EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='General Practitioner') AND EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='General Consultation')
BEGIN
    DECLARE @specId1 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Specialization WHERE Name='General Practitioner');
    DECLARE @svcId1 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Service WHERE Name='General Consultation');
    IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@specId1 AND ServiceId=@svcId1)
        INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@specId1, @svcId1);
END

-- Cardiologist -> Cardiology Review
IF EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Cardiologist') AND EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Cardiology Review')
BEGIN
    DECLARE @specId2 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Specialization WHERE Name='Cardiologist');
    DECLARE @svcId2 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Service WHERE Name='Cardiology Review');
    IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@specId2 AND ServiceId=@svcId2)
        INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@specId2, @svcId2);
END

-- Dermatologist -> Dermatology Check
IF EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Dermatologist') AND EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Dermatology Check')
BEGIN
    DECLARE @specId3 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Specialization WHERE Name='Dermatologist');
    DECLARE @svcId3 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Service WHERE Name='Dermatology Check');
    IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@specId3 AND ServiceId=@svcId3)
        INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@specId3, @svcId3);
END

-- Pediatrician -> Pediatric Visit
IF EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Pediatrician') AND EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Pediatric Visit')
BEGIN
    DECLARE @specId4 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Specialization WHERE Name='Pediatrician');
    DECLARE @svcId4 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Service WHERE Name='Pediatric Visit');
    IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@specId4 AND ServiceId=@svcId4)
        INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@specId4, @svcId4);
END

-- Orthopedist -> Orthopedic Assessment
IF EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Orthopedist') AND EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Orthopedic Assessment')
BEGIN
    DECLARE @specId5 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Specialization WHERE Name='Orthopedist');
    DECLARE @svcId5 nvarchar(36) = (SELECT Top 1 Id FROM practitioner.Service WHERE Name='Orthopedic Assessment');
    IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@specId5 AND ServiceId=@svcId5)
        INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@specId5, @svcId5);
END

-- Doctors (two sample doctors referencing existing user IDs if available)
IF NOT EXISTS (SELECT 1 FROM practitioner.Doctor)
BEGIN
    DECLARE @u1 nvarchar(36), @u2 nvarchar(36);
    -- Check if user.User table exists (shared database scenario)
    IF OBJECT_ID('[user].[User]', 'U') IS NOT NULL
    BEGIN
        SET @u1 = (SELECT TOP 1 Id FROM [user].[User] ORDER BY Created_At);
        SET @u2 = (SELECT TOP 1 Id FROM [user].[User] WHERE Id <> ISNULL(@u1,'') ORDER BY Created_At);
    END
    IF @u1 IS NULL SET @u1 = CONVERT(varchar(36),NEWID());
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
    DECLARE @rUser nvarchar(36);
    IF OBJECT_ID('[user].[User]', 'U') IS NOT NULL
        SET @rUser = (SELECT TOP 1 Id FROM [user].[User] WHERE Id NOT IN (SELECT UserId FROM practitioner.Doctor) ORDER BY Created_At);
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
