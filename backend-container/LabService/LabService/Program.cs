using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LabService.Data;
using LabService.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"] 
                     ?? builder.Configuration.GetConnectionString("MedicareDb") 
                     ?? builder.Configuration.GetConnectionString("LabDb") 
                     ?? throw new InvalidOperationException("No SQL connection string configured.");

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddDbContext<LabDbContext>((sp, options) =>
{
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "lab");
        sql.MigrationsAssembly(typeof(LabDbContext).Assembly.GetName().Name);
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
           var origins = allowed ?? Array.Empty<string>();
           if (origins.Length == 0) throw new InvalidOperationException("CORS AllowedOrigins must be configured in appsettings.");
        }
        p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Medicare Lab Service API", Version = "v1", Description = "Lab domain API" });
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

builder.Services.AddHealthChecks().AddDbContextCheck<LabDbContext>();

var app = builder.Build();
await DbSeeder.SeedAsync(app.Services);

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

app.MapDefaultEndpoints();

await app.RunAsync();
