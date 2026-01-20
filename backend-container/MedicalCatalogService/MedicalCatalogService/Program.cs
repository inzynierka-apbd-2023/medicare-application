using MedicalCatalogService.Data;
using MedicalCatalogService.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                        ?? builder.Configuration.GetConnectionString("MedicareDb") 
                        ?? builder.Configuration.GetConnectionString("MedicalCatalogDb") 
                        ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddDbContext<MedicalCatalogDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
        sql.MigrationsAssembly(typeof(MedicalCatalogDbContext).Assembly.GetName().Name);
        sql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<MedicalCatalogDbContext>();
builder.Services.AddControllers();

builder.AddMedicareMassTransit<MedicalCatalogDbContext>(x =>
{
    x.AddConsumer<MedicalCatalogService.Messaging.Consumers.AtcConsumer>();
    x.AddConsumer<MedicalCatalogService.Messaging.Consumers.LoincConsumer>();
});

builder.AddMedicareAuthentication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MedicalCatalog API", Version = "v1" });
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowed == null || allowed.Length == 0)
        {
            var origins = allowed ?? Array.Empty<string>();
            if (origins.Length == 0) throw new InvalidOperationException("CORS AllowedOrigins must be configured in appsettings.");
        }
        p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

app.UseGlobalExceptionHandling();

await DbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultPolicy");
app.UseDefaultRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
