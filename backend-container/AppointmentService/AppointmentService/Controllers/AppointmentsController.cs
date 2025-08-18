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

        var appointment = new Appointment
        {
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var patientIdStr = patientId.ToString();
        var appointments = await _db.Appointments
            .Where(a => a.PatientId == patientIdStr)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();
        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(string doctorId)
    {
        var appointments = await _db.Appointments
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();
        return Ok(appointments);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
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
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todaysAppointments = await _db.Appointments
            .Where(a => a.ScheduledAt >= today && a.ScheduledAt < tomorrow)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new { Date = today, Statistics = todaysAppointments });
    }
}

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime ScheduledEndAt, string? AppointmentType, string? Notes);
public record UpdateStatusRequest(string Status);
