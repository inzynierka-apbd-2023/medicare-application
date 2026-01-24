using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ArchiveService.Data;

namespace ArchiveService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ArchiveDbContext>>();
        
        const int maxRetries = 5;
        var delay = TimeSpan.FromSeconds(5);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully on attempt {Attempt}", attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries && 
                (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                 ex.Message.Contains("lock", StringComparison.OrdinalIgnoreCase) ||
                 ex.InnerException?.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true))
            {
                logger.LogWarning(ex, "Migration attempt {Attempt} failed due to timeout/lock. Retrying in {Delay}s...", 
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
            }
        }
    }
}
