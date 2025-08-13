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

var builder = WebApplication.CreateBuilder(args);

// Constants
const string UseAzureDefaultCredentialKey = "USE_AZURE_DEFAULT_CREDENTIAL";
const string AuthenticationKeyword = "Authentication";
const string AdminSeedUsername = "admin";

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
        options.UseSqlServer(sqlConn, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
    }
    else
    {
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
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

await app.RunAsync();

// --- Helpers ---
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

static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services, bool isDev)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations...");
        await db.Database.MigrateAsync();
        await SeedRolesAsync(db);
        if (isDev) await SeedAdminUserIfNoneAsync(db);
        Console.WriteLine("[Startup] Migrations & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
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
