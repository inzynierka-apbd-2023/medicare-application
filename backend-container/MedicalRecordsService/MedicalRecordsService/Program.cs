using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Data.SqlClient;
using MedicalRecordsService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                        ?? builder.Configuration.GetConnectionString("MedicareDb") 
                        ?? builder.Configuration.GetConnectionString("MedicalRecordsDb") 
                        ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();

builder.Services.AddDbContext<MedicalRecordsDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "medical");
        sql.MigrationsAssembly(typeof(MedicalRecordsDbContext).Assembly.GetName().Name);
    });
});

// Auth
builder.AddMedicareAuthentication();

// CORS
builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowed == null || allowed.Length == 0)
        {
            var origins = allowed ?? Array.Empty<string>();
        }
        p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Medical Records Service API", Version = "v1", Description = "Medical Records domain API" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<MedicalRecordsDbContext>();

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

await MedicalRecordsService.Data.Seeders.DbSeeder.SeedAsync(app.Services);

app.MapDefaultEndpoints();

await app.RunAsync();
