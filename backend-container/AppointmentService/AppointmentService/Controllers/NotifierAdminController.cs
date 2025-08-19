using System.Text;
using System.Text.Json;
using AppointmentService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/admin/notifier")]
public class NotifierAdminController : ControllerBase
{
    private readonly ILogger<NotifierAdminController> _logger;
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _config;

    public NotifierAdminController(ILogger<NotifierAdminController> logger, IServiceProvider sp, IConfiguration config)
    {
        _logger = logger; _sp = sp; _config = config;
    }

    [HttpPost("run-once")]
    public async Task<IActionResult> RunOnce()
    {
        try
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
            var now = DateTime.UtcNow;
            var windowEnd = now.AddHours(24);
            var due = await db.Appointments
                .Where(a => (a.Status == "Scheduled" || a.Status == "Confirmed")
                            && a.ScheduledAt >= now && a.ScheduledAt <= windowEnd
                            && a.UpcomingNotificationSentAt == null)
                .OrderBy(a => a.ScheduledAt)
                .Take(200)
                .ToListAsync();

            // Read RabbitMQ settings from configuration (env vars map __ -> :) 
            var host = _config["RABBITMQ:HOST"] ?? "rabbitmq";
            var user = _config["RABBITMQ:USERNAME"] ?? "guest";
            var pass = _config["RABBITMQ:PASSWORD"] ?? "guest";
            var factory = new ConnectionFactory { HostName = host, UserName = user, Password = pass };
            using var conn = factory.CreateConnection();
            using var ch = conn.CreateModel();
            var queue = "notifications.events";
            ch.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

            int published = 0;
            foreach (var appt in due)
            {
                var when = appt.ScheduledAt;
                var dateStr = when.ToString("yyyy-MM-dd");
                var timeStr = when.ToString("HH:mm");
                var message = $"Reminder: You have an appointment on {dateStr} at {timeStr}.";

                var evt = new { RecipientUserId = appt.PatientId, Description = message, Type = (byte)1, SourceService = "appointment-service", ActionUrl = $"/appointments/{appt.Id}", PriorityLevel = "Normal", ExpiresAt = (DateTime?)null };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
                var props = ch.CreateBasicProperties(); props.ContentType = "application/json"; props.DeliveryMode = 2;
                ch.BasicPublish(exchange: "", routingKey: queue, basicProperties: props, body: body);
                appt.UpcomingNotificationSentAt = DateTime.UtcNow;
                published++;
            }
            if (published > 0) await db.SaveChangesAsync();
            return Ok(new { published });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notifier admin run-once failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
