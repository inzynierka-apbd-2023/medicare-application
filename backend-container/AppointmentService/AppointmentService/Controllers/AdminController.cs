namespace AppointmentService.Controllers;
using Microsoft.AspNetCore.Authorization;
using AppointmentService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/appointment/admin")] 
[Authorize(Roles = "Owner,Admin")]
public class AdminController : ControllerBase
{
    private readonly AppointmentDbContext _db;
    public AdminController(AppointmentDbContext db) { _db = db; }

    // Reset the sent flag so the notifier can republish messages for the next 24 hours
    [HttpPost("reset-upcoming-flags")] 
    public async Task<IActionResult> ResetUpcomingFlags()
    {
        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(24);
        var affected = await _db.Appointments
            .Where(a => (a.Status == "Scheduled" || a.Status == "Confirmed")
                        && a.ScheduledAt >= now && a.ScheduledAt <= windowEnd)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.UpcomingNotificationSentAt, (DateTime?)null)
                .SetProperty(a => a.ThirtyMinNotificationSentAt, (DateTime?)null)
            );
        return Ok(new { reset = affected });
    }
    
    // Defensive cleanup to purge appointments by doctor id (supports either entity or user id)
    [HttpDelete("purge-appointments/{doctorId}")]
    public async Task<IActionResult> PurgeAppointments(Guid doctorId)
    {
        var q = _db.Appointments.Where(a => a.DoctorId == doctorId);
        _db.Appointments.RemoveRange(q);
        var deleted = await _db.SaveChangesAsync();
        return Ok(new { deleted });
    }
}
