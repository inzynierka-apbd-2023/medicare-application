using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Services;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<NotificationCleanupService> _logger;

    public NotificationCleanupService(IServiceProvider services, ILogger<NotificationCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
                var cutoff = DateTime.UtcNow.AddDays(-30);
                
                // delete read notifications older than 30 days or any expired ones
                await db.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID(N'[notifications].[Notification]') IS NOT NULL
                    BEGIN
                        DELETE FROM [notifications].[Notification]
                        WHERE (Is_Read = 1 AND Creation_Date < {0}) OR (Expires_At IS NOT NULL AND Expires_At < SYSUTCDATETIME());
                    END
                ", new object[] { cutoff }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification cleanup");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
