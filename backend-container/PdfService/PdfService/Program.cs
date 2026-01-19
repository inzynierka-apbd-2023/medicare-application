using PdfService.Services;
using PdfService.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add dependencies
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();
builder.Services.AddHostedService<PdfGenerationWorker>();

// Add Standard Middleware dependencies
builder.Services.AddControllers();
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

var app = builder.Build();

app.UseGlobalExceptionHandling();

app.UseRouting();
app.UseCors("DefaultPolicy");
app.UseDefaultRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints(); // Health checks

await app.RunAsync();

