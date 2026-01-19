using System.Text;
using System.Text.Json;
using AppointmentService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.AspNetCore.Authorization;
namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/admin/notifier")]
[Authorize(Roles = "Owner,Admin")]
public class NotifierAdminController : ControllerBase
{
    private readonly IServiceProvider _sp;
    private readonly IPublishEndpoint _publishEndpoint;

    public NotifierAdminController(IServiceProvider sp, IPublishEndpoint publishEndpoint)
    {
        _sp = sp; _publishEndpoint = publishEndpoint;
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

        int published = 0;
        foreach (var appt in due)
        {
            var when = appt.ScheduledAt;
            var dateStr = when.ToString("yyyy-MM-dd");
            var timeStr = when.ToString("HH:mm");
            var message = $"Reminder: You have an appointment on {dateStr} at {timeStr}.";

            await _publishEndpoint.Publish<INotificationCreated>(new
            {
                RecipientUserId = appt.PatientId,
                Description = message,
                Type = (byte)1,
                SourceService = "appointment-service",
                ActionUrl = $"/appointments/{appt.Id}",
                PriorityLevel = "Normal",
                ExpiresAt = (DateTime?)null
            });

            appt.UpcomingNotificationSentAt = DateTime.UtcNow;
            published++;
        }
        if (published > 0) await db.SaveChangesAsync();
        return Ok(new { published });
    }
}
