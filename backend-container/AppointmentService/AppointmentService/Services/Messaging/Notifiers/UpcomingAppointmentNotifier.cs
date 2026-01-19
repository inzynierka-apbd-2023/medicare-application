using AppointmentService.Data;
using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppointmentService.Services.Messaging.Notifiers;

public class UpcomingAppointmentNotifier : BackgroundService
{
    private readonly ILogger<UpcomingAppointmentNotifier> _logger;
    private readonly IServiceProvider _sp;

    public UpcomingAppointmentNotifier(ILogger<UpcomingAppointmentNotifier> logger, IServiceProvider sp)
    {
        _logger = logger;
        _sp = sp;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                
                var now = DateTime.UtcNow;
                var windowStart = now;
                var windowEnd = now.AddHours(24);

                var due = await db.Appointments
                    .Where(a => (a.Status == "Scheduled" || a.Status == "Confirmed")
                                && a.ScheduledAt >= windowStart && a.ScheduledAt <= windowEnd
                                && a.UpcomingNotificationSentAt == null)
                    .OrderBy(a => a.ScheduledAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var appt in due)
                {
                    var when = appt.ScheduledAt;
                    var dateStr = when.ToString("yyyy-MM-dd");
                    var timeStr = when.ToString("HH:mm");
                    var message = $"Reminder: You have an appointment on {dateStr} at {timeStr}.";

                    try
                    {
                        await publishEndpoint.Publish<INotificationCreated>(new
                        {
                            RecipientUserId = appt.PatientId,
                            Description = message,
                            Type = (byte)1,
                            SourceService = "appointment-service",
                            ActionUrl = $"/appointments/{appt.Id}",
                            PriorityLevel = "Normal",
                            ExpiresAt = (DateTime?)appt.ScheduledEndAt
                        }, stoppingToken);

                        appt.UpcomingNotificationSentAt = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken); // Commit change (and Outbox message if configured, although Publish inside loop might create many Outbox messages explicitly if not saving)
                        // If we use Outbox on DbContext, publish just adds entity. SaveChanges commits it.
                        // Here we do SaveChanges for every item. This is fine for 100 items. Be aware of N+1 but it's okay for background job.
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish upcoming appointment notification for {Id}", appt.Id);
                    }
                }

                // 30-minute reminders
                var soonWindowStart = now.AddMinutes(25);
                var soonWindowEnd = now.AddMinutes(35);
                var due30 = await db.Appointments
                    .Where(a => (a.Status == "Scheduled" || a.Status == "Confirmed")
                                && a.ScheduledAt >= soonWindowStart && a.ScheduledAt <= soonWindowEnd
                                && a.ThirtyMinNotificationSentAt == null)
                    .OrderBy(a => a.ScheduledAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var appt in due30)
                {
                    var when = appt.ScheduledAt;
                    var dateStr = when.ToString("yyyy-MM-dd");
                    var timeStr = when.ToString("HH:mm");
                    var message = $"Reminder: You have an appointment in 30 minutes (at {timeStr} on {dateStr}).";

                    try
                    {
                        await publishEndpoint.Publish<INotificationCreated>(new
                        {
                            RecipientUserId = appt.PatientId,
                            Description = message,
                            Type = (byte)1,
                            SourceService = "appointment-service",
                            ActionUrl = $"/appointments/{appt.Id}",
                            PriorityLevel = "Normal",
                            ExpiresAt = (DateTime?)appt.ScheduledEndAt
                        }, stoppingToken);

                        appt.ThirtyMinNotificationSentAt = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish 30-min appointment reminder for {Id}", appt.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpcomingAppointmentNotifier tick failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
