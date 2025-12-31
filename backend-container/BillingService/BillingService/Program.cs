using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;

using System.Text;
using System.Security.Claims;
using BillingService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string AuthenticationKeyword = "Authentication";

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                     ?? builder.Configuration.GetConnectionString("MedicareDb") 
                     ?? builder.Configuration.GetConnectionString("BillingDb") 
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

LogConnectionInfo(connectionString, "Config");

builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services
builder.Services.AddScoped<BillingService.Services.IRevenueMetricsService, BillingService.Services.RevenueMetricsService>();

builder.Services.AddDbContext<BillingDbContext>((sp, options) =>
{
    // Suppress EF Core 9 PendingModelChangesWarning for local development
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing");
        sql.MigrationsAssembly(typeof(BillingDbContext).Assembly.GetName().Name);
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
    o.AddPolicy("DefaultPolicy", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing Service API", Version = "v1" });
});

builder.Services.AddHealthChecks().AddDbContextCheck<BillingDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
if (!app.Environment.IsProduction() && string.Equals(app.Configuration["BILLING_BYPASS_AUTH"], "true", StringComparison.OrdinalIgnoreCase))
{
    app.Use(async (ctx, next) =>
    {
        if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                new Claim(ClaimTypes.Name, "BillingService Dev User")
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
}

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
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    try
    {
        Console.WriteLine("[Startup] Applying EF Core migrations (Billing)...");
        await db.Database.MigrateAsync();
        await CreateViewsAsync(db);
        await BillingService.Data.MockDataSeeder.SeedAsync(db);
        Console.WriteLine("[Startup] Billing migrations complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Billing migration failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Startup] Inner: {ex.InnerException.Message}");
    }
}

static async Task CreateViewsAsync(BillingDbContext db)
{
    try
    {
        // Create billing summary views - these are simple aggregates on billing tables only
        // Note: Status enum - 0=Pending, 1=RequiresAction, 2=Failed, 3=Succeeded, 4=Cancelled, 5=RefundedFull, 6=RefundedPartial
        var patientSummaryView = @"
CREATE OR ALTER VIEW billing.vw_Patient_Billing_Summary AS
SELECT 
    pm.PatientId,
    COUNT(DISTINCT pi.Id) AS TotalPaymentIntents,
    SUM(CASE WHEN pi.Status = 3 THEN pi.AmountCents ELSE 0 END) AS TotalPaidAmount,
    SUM(CASE WHEN pi.Status = 0 THEN pi.AmountCents ELSE 0 END) AS TotalPendingAmount,
    MAX(pi.CreatedAt) AS LastPaymentDate
FROM billing.Payment_Method pm
LEFT JOIN billing.Payment_Intent pi ON pi.PatientId = pm.PatientId
GROUP BY pm.PatientId;
";
        await db.Database.ExecuteSqlRawAsync(patientSummaryView);

        var doctorRevenueView = @"
CREATE OR ALTER VIEW billing.vw_Doctor_Revenue_Dashboard AS
SELECT 
    CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS DoctorId,
    COUNT(ap.Id) AS TotalAppointmentPayments,
    SUM(ap.AmountCents) AS TotalRevenue,
    AVG(ap.AmountCents) AS AveragePaymentAmount
FROM billing.Appointment_Payment ap
GROUP BY PatientId;
";
        await db.Database.ExecuteSqlRawAsync(doctorRevenueView);
        
        Console.WriteLine("[Startup] Billing views created.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] View creation warning: {ex.Message}");
    }
}
