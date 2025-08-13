using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Data;
using MedicalCatalogService.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var useAzureDefaultCredential = (builder.Configuration["USE_AZURE_DEFAULT_CREDENTIAL"] ?? builder.Configuration["UseAzureDefaultCredential"])?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

builder.Services.AddDbContext<MedicalCatalogDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
        sql.MigrationsAssembly(typeof(MedicalCatalogDbContext).Assembly.GetName().Name);
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<MedicalCatalogDbContext>();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CatalogImport", policy =>
    {
        // In Development, allow anonymous for import endpoints; otherwise require auth
        if ((builder.Environment?.IsDevelopment() ?? false) || (builder.Environment?.EnvironmentName == "Test"))
        {
            policy.RequireAssertion(_ => true);
        }
        else
        {
            policy.RequireAuthenticatedUser();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MedicalCatalog API", Version = "v1" });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MedicalCatalogDbContext>();

    if (useAzureDefaultCredential)
    {
        Console.WriteLine("[Startup] Normalized connection string for AAD token (removed credentials / Authentication).");
        var csb = new SqlConnectionStringBuilder(connectionString)
        {
            UserID = string.Empty,
            Password = string.Empty,
            Authentication = SqlAuthenticationMethod.NotSpecified
        };
        var normalized = csb.ToString();
        await using var conn = new SqlConnection(normalized);
        var credential = new DefaultAzureCredential();
        var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
        conn.AccessToken = token.Token;
        await conn.OpenAsync();
        await conn.CloseAsync();
        Console.WriteLine("[Startup] Using AAD token for SQL connection test.");
    }

    Console.WriteLine("[Startup] Applying EF Core migrations (MedicalCatalog)...");
    await db.Database.MigrateAsync();
}

app.Run();
