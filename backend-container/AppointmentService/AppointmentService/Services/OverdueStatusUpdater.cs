using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Services;

public class OverdueStatusUpdater : BackgroundService
{
    private readonly ILogger<OverdueStatusUpdater> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OverdueStatusUpdater(ILogger<OverdueStatusUpdater> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueStatusUpdater started");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();

            var now = DateTime.Now;

            var updated = await db.Appointments
                .Where(a => (a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, a => "Overdue")
                    .SetProperty(a => a.UpdatedAt, a => DateTime.UtcNow),
                    cancellationToken: stoppingToken);

            if (updated > 0)
            {
                _logger.LogInformation("OverdueStatusUpdater: marked {Count} appointments as Overdue", updated);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("OverdueStatusUpdater stopped");
    }
}
