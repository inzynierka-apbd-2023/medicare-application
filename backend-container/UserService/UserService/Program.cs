using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserService.Data;
using UserService.Services;
using UserService.Models;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UserService.Infrastructure.Messaging;


// Constants (Must be top-level for static local functions to see them if they are const)
const string AuthenticationKeyword = "Authentication";

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    // Database
    var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                         ?? builder.Configuration.GetConnectionString("MedicareDb") 
                         ?? builder.Configuration.GetConnectionString("UserServiceDb") 
                         ?? throw new InvalidOperationException("No SQL connection string configured.");

    LogConnectionInfo(connectionString, "Config");
    
    // Controllers
    builder.Services.AddControllers();

    builder.Services.AddDbContext<UserDbContext>((sp, options) =>
    {
        // Suppress EF Core 9 PendingModelChangesWarning for local development
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsHistoryTable("__EFMigrationsHistory", null);
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

    // CORS
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("DefaultPolicy", p =>
        {
            var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
            p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
    });

    // Services
    builder.Services.AddScoped<IUserService, UserServiceImpl>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.AddRabbitMQClient("rabbitmq");
    builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RABBITMQ"));
    builder.Services.AddHostedService<OutboxPublisherHostedService>();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare User Service API", Version = "v1", Description = "User management API" });
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

    // Health checks
    builder.Services.AddHealthChecks().AddDbContextCheck<UserDbContext>();

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

    // Always apply migrations to ensure DB exists in Azure (Production)
    await ApplyMigrationsAndSeedAsync(app.Services, app.Environment.IsDevelopment());

    app.MapDefaultEndpoints();

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL] Application terminated unexpectedly: {ex}");
    await Task.Delay(10000); // Wait 10s to ensure logs are flushed/read
    throw;
}

// --- Helpers ---
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

static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services, bool isDev)
{
    using var scope = services.CreateScope();
    var servicesProvider = scope.ServiceProvider;
    var logger = servicesProvider.GetRequiredService<ILogger<Program>>();
    var db = servicesProvider.GetRequiredService<UserDbContext>();
    
    int retries = 0;
    while (true)
    {
        try
        {
            Console.WriteLine("[Startup] Applying EF Core migrations...");
            // Pre-migration safeguard: ensure [user] schema exists and transfer tables if still under dbo.
            var transferSql = @"IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'user') EXEC('CREATE SCHEMA [user]');
IF OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL AND OBJECT_ID('[user].[Role]', 'U') IS NULL ALTER SCHEMA [user] TRANSFER [dbo].[Role];
IF OBJECT_ID('[dbo].[User]', 'U') IS NOT NULL AND OBJECT_ID('[user].[User]', 'U') IS NULL ALTER SCHEMA [user] TRANSFER [dbo].[User];
IF OBJECT_ID('[dbo].[User_Profile]', 'U') IS NOT NULL AND OBJECT_ID('[user].[User_Profile]', 'U') IS NULL ALTER SCHEMA [user] TRANSFER [dbo].[User_Profile];";
            try
            {
                await db.Database.ExecuteSqlRawAsync(transferSql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Pre-migration transfer warning: {ex.Message}");
            }
            await db.Database.MigrateAsync();

            // Post-migration fallback: create Refresh_Token table if migration was recorded but table missing (observed prod drift scenario)
            var createRefreshSql = @"IF OBJECT_ID('[user].[Refresh_Token]', 'U') IS NULL
BEGIN
    PRINT('[Startup] Refresh_Token table missing; creating fallback table.');
    CREATE TABLE [user].[Refresh_Token](
        [Id] uniqueidentifier NOT NULL CONSTRAINT DF_RefreshToken_Id DEFAULT NEWID(),
        [User_Id] uniqueidentifier NOT NULL,
        [Token_Hash] nvarchar(128) NOT NULL,
        [Expires_At] datetime2 NOT NULL CONSTRAINT DF_RefreshToken_Expires DEFAULT DATEADD(day,7,SYSUTCDATETIME()),
        [Created_At] datetime2 NOT NULL CONSTRAINT DF_RefreshToken_Created DEFAULT SYSUTCDATETIME(),
        [Revoked_At] datetime2 NULL,
        [Replaced_By_Hash] nvarchar(128) NULL,
        [Created_By_Ip] nvarchar(45) NULL,
        [Revoked_By_Ip] nvarchar(45) NULL,
        [User_Agent] nvarchar(512) NULL,
        CONSTRAINT [PK_Refresh_Token] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Refresh_Token_User_User_Id] FOREIGN KEY ([User_Id]) REFERENCES [user].[User]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Refresh_Token_User_Id_Expires_At] ON [user].[Refresh_Token]([User_Id],[Expires_At]);
END";
            try
            {
                await db.Database.ExecuteSqlRawAsync(createRefreshSql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Startup] Fallback Refresh_Token create failed: {ex.Message}");
            }
            await SeedRolesAsync(db);
            await SeedDevelopmentUsersAsync(db);
            await UserService.Data.MockDataSeeder.SeedAsync(db);
            Console.WriteLine("[Startup] Migrations & seeding complete.");
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($"[Startup] Migration failed: {ex.Message}. Retry {retries}...");
            if (retries > 10) throw;
            await Task.Delay(5000);
        }
    }
}

static async Task SeedRolesAsync(UserDbContext db)
{
    var existingRoles = await db.Roles.Select(r => r.Name).ToListAsync();
    var rolesToAdd = new List<Role>();
    
    if (!existingRoles.Contains("Admin"))
        rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator" });
    if (!existingRoles.Contains("Doctor"))
        rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Doctor", Description = "Doctor user" });
    if (!existingRoles.Contains("Patient"))
        rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Patient", Description = "Patient user" });
    if (!existingRoles.Contains("Receptionist"))
        rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Receptionist", Description = "Receptionist user" });
    if (!existingRoles.Contains("Owner"))
        rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Owner", Description = "Clinic owner" });
    
    if (rolesToAdd.Any())
    {
        db.Roles.AddRange(rolesToAdd);
        await db.SaveChangesAsync();
        Console.WriteLine($"[Startup] Seeded {rolesToAdd.Count} roles: {string.Join(", ", rolesToAdd.Select(r => r.Name))}");
    }
}

static async Task SeedDevelopmentUsersAsync(UserDbContext db)
{
    // Development test users with known passwords and DETERMINISTIC IDs
    // These IDs must match MockIds in other services for cross-service references
    var testUsers = new[]
    {
        new { Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001"), Username = "patient_a_20250818", Password = "P@ssw0rd!", Role = "Patient", FirstName = "Test", LastName = "Patient", Email = "patient@test.local" },
        new { Id = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001"), Username = "doctor_a_20250818", Password = "P@ssw0rd!", Role = "Doctor", FirstName = "Test", LastName = "Doctor", Email = "doctor@test.local" },
        new { Id = Guid.Parse("cccccccc-0003-0003-0003-000000000001"), Username = "reception_a_20250818", Password = "P@ssw0rd!", Role = "Receptionist", FirstName = "Test", LastName = "Receptionist", Email = "reception@test.local" },
        new { Id = Guid.Parse("dddddddd-0004-0004-0004-000000000001"), Username = "admin_a_20250818", Password = "P@ssw0rd!", Role = "Admin", FirstName = "Test", LastName = "Admin", Email = "admin@test.local" },
        new { Id = Guid.Parse("eeeeeeee-0005-0005-0005-000000000001"), Username = "owner@test.local", Password = "P@ssw0rd!", Role = "Owner", FirstName = "Test", LastName = "Owner", Email = "owner@test.local" },
    };

    var existingUsernames = await db.Users.Select(u => u.Username).ToListAsync();
    var roles = await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);
    int created = 0;

    foreach (var tu in testUsers)
    {
        if (existingUsernames.Contains(tu.Username)) continue;
        if (!roles.TryGetValue(tu.Role, out var roleId)) continue;

        db.Users.Add(new User
        {
            Id = tu.Id,
            Username = tu.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tu.Password),
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.UserProfiles.Add(new UserProfile
        {
            UserId = tu.Id,
            FirstName = tu.FirstName,
            LastName = tu.LastName,
            Email = tu.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        created++;
    }

    if (created > 0)
    {
        await db.SaveChangesAsync();
        Console.WriteLine($"[Startup] Seeded {created} development test users:");
        foreach (var tu in testUsers)
        {
            if (!existingUsernames.Contains(tu.Username))
                Console.WriteLine($"  - {tu.Username} / {tu.Password} ({tu.Role})");
        }
    }
}
