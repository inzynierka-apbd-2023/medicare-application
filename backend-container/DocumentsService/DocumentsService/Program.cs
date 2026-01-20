using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DocumentsService.Data;
using DocumentsService.Data.Seeders;
using Medicare.Messaging.Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                        ?? builder.Configuration.GetConnectionString("MedicareDb") 
                        ?? builder.Configuration.GetConnectionString("DocumentsDb") 
                        ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();

builder.AddMedicareMassTransit<DocumentsDbContext>(x =>
{
    x.AddRequestClient<IGeneratePdfRequest>();
    x.AddRequestClient<IGetDoctor>();
    x.AddRequestClient<IGetPatient>();
    x.AddRequestClient<IGetAtc>();
    x.AddRequestClient<IGetLoinc>();
});

builder.Services.AddDbContext<DocumentsDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
        sql.MigrationsAssembly(typeof(DocumentsDbContext).Assembly.GetName().Name);
    });
});

builder.AddMedicareAuthentication();

builder.Services.AddCors(o =>
{
    o.AddPolicy("DefaultPolicy", p =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowed == null || allowed.Length == 0)
        {
            throw new InvalidOperationException("CORS AllowedOrigins must be configured in appsettings.");
        }
        p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
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

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseDefaultRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await DbSeeder.SeedAsync(app.Services);

app.MapDefaultEndpoints();

await app.RunAsync();
