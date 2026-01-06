using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Services;

/// <summary>
/// Doctor schedule service - simplified to match patient pattern.
/// Returns appointments without cross-schema enrichment.
/// Frontend can enrich with patient data if needed (same as patient view enriches doctor data).
/// </summary>
public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly AppointmentDbContext _context;

    public DoctorScheduleService(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorScheduleResponse> GetDoctorScheduleAsync(
        Guid doctorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments.Where(a => a.DoctorId == doctorId);

        if (startDate.HasValue)
            query = query.Where(a => a.ScheduledAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.ScheduledAt <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var appointments = await query
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync(cancellationToken);

        // Update overdue status in-memory (same as AppointmentsController does)
        var now = DateTime.Now;
        foreach (var a in appointments)
        {
            if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
            {
                a.Status = "Overdue";
            }
        }

        // Map to DTO - simplified without cross-schema patient enrichment
        var scheduleEvents = appointments.Select(MapToScheduleEvent).ToList();

        return new DoctorScheduleResponse
        {
            Schedule = scheduleEvents,
            TotalCount = scheduleEvents.Count
        };
    }

    public async Task<DoctorScheduleResponse> GetTodaysAppointmentsAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await GetDoctorScheduleAsync(doctorId, today, today.AddDays(1).AddTicks(-1), null, cancellationToken);
    }

    public async Task<DoctorScheduleEventDto?> GetAppointmentDetailsAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment == null)
            return null;

        return MapToScheduleEvent(appointment);
    }

    public async Task<bool> UpdateAppointmentStatusAsync(
        Guid appointmentId,
        string status,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment == null)
            return false;

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            appointment.Notes = notes;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddAppointmentNotesAsync(
        Guid appointmentId,
        string notes,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment == null)
            return false;

        appointment.Notes = string.IsNullOrEmpty(appointment.Notes)
            ? notes
            : appointment.Notes + Environment.NewLine + notes;

        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Maps appointment to DTO without cross-schema patient enrichment.
    /// Patient name/phone will show "Unknown" - frontend can enrich from UserService if needed.
    /// This matches how patient scheduler works (it enriches doctor name from PractitionerService on frontend).
    /// </summary>
    private static DoctorScheduleEventDto MapToScheduleEvent(AppointmentService.Models.Appointment appointment)
    {
        return new DoctorScheduleEventDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = "Patient", // Frontend can enrich from UserService
            PatientAge = 0,
            PatientPhone = "",
            PatientEmail = null,
            AppointmentType = appointment.AppointmentType ?? "General",
            Date = appointment.ScheduledAt.ToString("yyyy-MM-dd"),
            Time = appointment.ScheduledAt.ToString("HH:mm"),
            Duration = (int)(appointment.ScheduledEndAt - appointment.ScheduledAt).TotalMinutes,
            Status = appointment.Status.ToLower(),
            ChiefComplaint = appointment.ChiefComplaint,
            Notes = appointment.Notes,
            MedicalHistory = new List<string>(),
            Allergies = new List<string>(),
            CurrentMedications = new List<string>()
        };
    }
}
