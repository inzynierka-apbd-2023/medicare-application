using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Models;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly AppointmentDbContext _db;
    public SchedulesController(AppointmentDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest req)
    {
        var schedule = new Schedule
        {
            DoctorId = req.DoctorId,
            DayOfWeek = req.DayOfWeek,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Schedules.Add(schedule);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByDoctorId), new { doctorId = schedule.DoctorId }, schedule);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(string doctorId)
    {
        var schedules = await _db.Schedules
            .Where(s => s.DoctorId == doctorId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();
        return Ok(schedules);
    }

    [HttpGet("slots/{doctorId}")]
    public async Task<IActionResult> GetAvailableSlots(string doctorId, DateTime date)
    {
        var slots = await _db.AppointmentSlots
            .Where(s => s.DoctorId == doctorId 
                && s.StartTime.Date == date.Date 
                && s.IsAvailable)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
        return Ok(slots);
    }
}

public record CreateScheduleRequest(string DoctorId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
