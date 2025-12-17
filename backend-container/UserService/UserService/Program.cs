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
const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";
const string AdminSeedUsername = "admin";

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    // Normalize connection early
    var (connectionString, connectionSource, useAzureDefaultCredential) = NormalizeConnectionString(builder.Configuration);
    LogConnectionInfo(connectionString, connectionSource);

    // Controllers
    builder.Services.AddControllers();

    // Database
    if (useAzureDefaultCredential)
    {
        builder.Services.AddScoped(_ => CreateTokenSqlConnection(connectionString));
    }

    builder.Services.AddDbContext<UserDbContext>((sp, options) =>
    {
        if (useAzureDefaultCredential)
        {
            var sqlConn = sp.GetRequiredService<SqlConnection>();
            options.UseSqlServer(sqlConn, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", null);
            });
        }
        else
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", null);
            });
        }
    });

    // Auth (JWT)
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

    if (!app.Environment.IsProduction())
    {
        await ApplyMigrationsAndSeedAsync(app.Services, app.Environment.IsDevelopment());
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

// --- Helpers ---
static (string ConnectionString, string Source, bool UseAzureDefaultCredential) NormalizeConnectionString(IConfiguration config)
{
    string? src; string? cs;
    if (!string.IsNullOrWhiteSpace(config["AZURE_SQL_CONNECTIONSTRING"])) { cs = config["AZURE_SQL_CONNECTIONSTRING"]; src = "AZURE_SQL_CONNECTIONSTRING"; }
    else if (!string.IsNullOrWhiteSpace(config["ConnectionStrings__UserServiceDb"])) { cs = config["ConnectionStrings__UserServiceDb"]; src = "ConnectionStrings__UserServiceDb env var"; }
    else { cs = config.GetConnectionString("UserServiceDb"); src = "appsettings"; }
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
        // For local development with SQL Server containers, trusting the server certificate is often required
        // to avoid "The remote certificate was invalid" or pre-login handshake errors.
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
        [Id] nvarchar(450) NOT NULL CONSTRAINT DF_RefreshToken_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
        [User_Id] nvarchar(450) NOT NULL,
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
            if (isDev) await SeedAdminUserIfNoneAsync(db);
            await SeedTestPatientAsync(db);
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
    if (await db.Roles.AnyAsync()) return;
    Console.WriteLine("[Startup] Seeding roles...");
    db.Roles.AddRange(
        new Role { Id = Guid.NewGuid().ToString(), Name = "Admin", Description = "Administrator" },
        new Role { Id = Guid.NewGuid().ToString(), Name = "Doctor", Description = "Doctor user" },
        new Role { Id = Guid.NewGuid().ToString(), Name = "Patient", Description = "Patient user" }
    );
    await db.SaveChangesAsync();
}

static async Task SeedAdminUserIfNoneAsync(UserDbContext db)
{
    if (await db.Users.AnyAsync()) return;
    var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole == null) return;
    var tempPassword = ($"Adm!n-{Guid.NewGuid():N}").Substring(0, 16);
    var userId = Guid.NewGuid().ToString();
    db.Users.Add(new User
    {
        Id = userId,
        Username = AdminSeedUsername,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
        RoleId = adminRole.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsActive = true
    });
    db.UserProfiles.Add(new UserProfile
    {
        UserId = userId,
        FirstName = "System",
        LastName = "Admin",
        Email = "admin@local.invalid",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });
    await db.SaveChangesAsync();
    Console.WriteLine($"[Startup] Seeded admin user. Username: {AdminSeedUsername} TempPassword: {tempPassword}");
}

static async Task SeedTestPatientAsync(UserDbContext db)
{
    // Username from test-users.txt (example). Only seed details if user already exists.
    const string testUsername = "patient_a_20250818"; // adjust if date pattern changes
    var user = await db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Username == testUsername);
    if (user == null || user.Profile == null) return; // don't create; only enrich existing test user
    bool changed = false;
    if (string.IsNullOrWhiteSpace(user.Profile.Phone)) { user.Profile.Phone = "+1-555-0100"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.AddressLine1)) { user.Profile.AddressLine1 = "123 Test Street"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.AddressLine2)) { user.Profile.AddressLine2 = "Apt 4B"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.City)) { user.Profile.City = "Testville"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.State)) { user.Profile.State = "TS"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.ZipCode)) { user.Profile.ZipCode = "12345"; changed = true; }
    if (string.IsNullOrWhiteSpace(user.Profile.Country)) { user.Profile.Country = "Testland"; changed = true; }
    if (changed)
    {
        user.Profile.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        Console.WriteLine($"[Startup] Enriched test patient '{testUsername}' with phone/address.");
    }
}
