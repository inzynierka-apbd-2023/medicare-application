using System.Text;
using System.Text.Json;
using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AppointmentService.Services;

public class UpcomingAppointmentNotifier : BackgroundService
{
    private readonly ILogger<UpcomingAppointmentNotifier> _logger;
    private readonly IServiceProvider _sp;
    private readonly IConnection _conn;
    private IModel? _ch;
    private readonly string _queue = "notifications.events";

    public UpcomingAppointmentNotifier(ILogger<UpcomingAppointmentNotifier> logger, IServiceProvider sp, IConnection conn)
    {
        _logger = logger;
        _sp = sp;
        _conn = conn;
    }

    private void EnsureRabbitChannel()
    {
        if (_ch != null && _ch.IsOpen) return;
        try
        {
            _ch = _conn.CreateModel();
            _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation("UpcomingAppointmentNotifier channel created for queue {Queue}", _queue);
        }
        catch (Exception ex)
        {
            _ch = null;
            _logger.LogError(ex, "Failed to create RabbitMQ channel");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    // Initial connect (will also be retried each tick)
        try { EnsureRabbitChannel(); } catch (Exception ex) { _logger.LogError(ex, "Initial RabbitMQ connection failed; will retry."); }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
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

                if (due.Count > 0)
                {
                    _logger.LogInformation("UpcomingAppointmentNotifier: found {Count} appointment(s) due for notification", due.Count);
                }

                foreach (var appt in due)
                {
                    var when = appt.ScheduledAt;
                    var dateStr = when.ToString("yyyy-MM-dd");
                    var timeStr = when.ToString("HH:mm");
                    var message = $"Reminder: You have an appointment on {dateStr} at {timeStr}.";

                    var evt = new
                    {
                        RecipientUserId = appt.PatientId,
                        Description = message,
                        Type = (byte)1,
                        SourceService = "appointment-service",
                        ActionUrl = $"/appointments/{appt.Id}",
                        PriorityLevel = "Normal",
                        ExpiresAt = (DateTime?)appt.ScheduledEndAt
                    };

                    try
                    {
                        EnsureRabbitChannel();
                        if (_ch == null)
                        {
                            _logger.LogWarning("RabbitMQ channel not available; will retry next tick for appointment {Id}", appt.Id);
                        }
                        else
                        {
                            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
                            var props = _ch.CreateBasicProperties();
                            props.ContentType = "application/json";
                            props.DeliveryMode = 2;
                            _ch.BasicPublish(exchange: "", routingKey: _queue, basicProperties: props, body: body);

                            appt.UpcomingNotificationSentAt = DateTime.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                        }
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

                if (due30.Count > 0)
                {
                    _logger.LogInformation("UpcomingAppointmentNotifier: found {Count} appointment(s) for 30-min reminder", due30.Count);
                }

                foreach (var appt in due30)
                {
                    var when = appt.ScheduledAt;
                    var dateStr = when.ToString("yyyy-MM-dd");
                    var timeStr = when.ToString("HH:mm");
                    var message = $"Reminder: You have an appointment in 30 minutes (at {timeStr} on {dateStr}).";

                    var evt = new
                    {
                        RecipientUserId = appt.PatientId,
                        Description = message,
                        Type = (byte)1,
                        SourceService = "appointment-service",
                        ActionUrl = $"/appointments/{appt.Id}",
                        PriorityLevel = "Normal",
                        ExpiresAt = (DateTime?)appt.ScheduledEndAt
                    };

                    try
                    {
                        EnsureRabbitChannel();
                        if (_ch == null)
                        {
                            _logger.LogWarning("RabbitMQ channel not available; will retry next tick for 30-min reminder appointment {Id}", appt.Id);
                        }
                        else
                        {
                            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
                            var props = _ch.CreateBasicProperties();
                            props.ContentType = "application/json";
                            props.DeliveryMode = 2;
                            _ch.BasicPublish(exchange: "", routingKey: _queue, basicProperties: props, body: body);

                            appt.ThirtyMinNotificationSentAt = DateTime.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                        }
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

    public override void Dispose()
    {
        base.Dispose();
        try { _ch?.Close(); _ch?.Dispose(); } catch { }
        // Do not dispose injected _conn!
    }
}
