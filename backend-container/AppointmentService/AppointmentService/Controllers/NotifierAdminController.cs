using System.Text;
using System.Text.Json;
using AppointmentService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/admin/notifier")]
[Authorize(Roles = "Owner,Admin")]
public class NotifierAdminController : ControllerBase
{
    private readonly ILogger<NotifierAdminController> _logger;
    private readonly IServiceProvider _sp;
    private readonly IConnection _rabbitConnection;

    public NotifierAdminController(ILogger<NotifierAdminController> logger, IServiceProvider sp, IConnection rabbitConnection)
    {
        _logger = logger; _sp = sp; _rabbitConnection = rabbitConnection;
    }

    [HttpPost("run-once")]
    public async Task<IActionResult> RunOnce()
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

        await using var ch = await _rabbitConnection.CreateChannelAsync();
        var queue = "notifications.events";
        await ch.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);

        int published = 0;
        foreach (var appt in due)
        {
            var when = appt.ScheduledAt;
            var dateStr = when.ToString("yyyy-MM-dd");
            var timeStr = when.ToString("HH:mm");
            var message = $"Reminder: You have an appointment on {dateStr} at {timeStr}.";

            var evt = new { RecipientUserId = appt.PatientId, Description = message, Type = (byte)1, SourceService = "appointment-service", ActionUrl = $"/appointments/{appt.Id}", PriorityLevel = "Normal", ExpiresAt = (DateTime?)null };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
            var props = new BasicProperties(); 
            props.ContentType = "application/json"; 
            props.DeliveryMode = DeliveryModes.Persistent;
            
            await ch.BasicPublishAsync(exchange: "", routingKey: queue, mandatory: false, basicProperties: props, body: body);
            appt.UpcomingNotificationSentAt = DateTime.UtcNow;
            published++;
        }
        if (published > 0) await db.SaveChangesAsync();
        return Ok(new { published });
    }
}
