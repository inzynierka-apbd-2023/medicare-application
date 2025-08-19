using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Models;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentDbContext _db;
    public AppointmentsController(AppointmentDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest req)
    {
        if (req.PatientId == Guid.Empty || req.DoctorId == Guid.Empty)
            return BadRequest("PatientId and DoctorId are required");
        if (req.ScheduledAt == default || req.ScheduledEndAt == default || req.ScheduledEndAt <= req.ScheduledAt)
            return BadRequest("Invalid scheduled times");

        try
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = req.PatientId.ToString(),
                DoctorId = req.DoctorId.ToString(),
                ScheduledAt = req.ScheduledAt,
                ScheduledEndAt = req.ScheduledEndAt,
                AppointmentType = req.AppointmentType,
                Notes = req.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "Database update failed", error = ex.Message, inner = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to create appointment", error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        var now = DateTime.Now;
        if ((appointment.Status == "Scheduled" || appointment.Status == "Confirmed") && appointment.ScheduledEndAt < now)
        {
            // Effective overdue on read
            appointment.Status = "Overdue";
        }
        return Ok(appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        try
        {
            var patientIdStr = patientId.ToString();
            var now = DateTime.Now;
            var appointments = await _db.Appointments
                .Where(a => a.PatientId == patientIdStr)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();
            foreach (var a in appointments)
            {
                if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
                {
                    a.Status = "Overdue";
                }
            }
            return Ok(appointments);
        }
        catch
        {
            // Graceful fallback if database not ready
            return Ok(Array.Empty<Appointment>());
        }
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(string doctorId)
    {
        try
        {
            var now = DateTime.Now;
            var appointments = await _db.Appointments
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();
            foreach (var a in appointments)
            {
                if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
                {
                    a.Status = "Overdue";
                }
            }
            return Ok(appointments);
        }
        catch
        {
            return Ok(Array.Empty<Appointment>());
        }
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = req.Status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(appointment);
    }

    [HttpGet("analytics/today")]
    public async Task<IActionResult> GetTodaysAnalytics()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todaysAppointments = await _db.Appointments
                .Where(a => a.ScheduledAt >= today && a.ScheduledAt < tomorrow)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new { Date = today, Statistics = todaysAppointments });
        }
        catch
        {
            return Ok(new { Date = DateTime.Today, Statistics = Array.Empty<object>() });
        }
    }
}

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime ScheduledEndAt, string? AppointmentType, string? Notes);
public record UpdateStatusRequest(string Status);
