using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Services;

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly AppointmentDbContext _context;
    private readonly IPatientService _patientService;
    private readonly IMedicalRecordsService _medicalRecordsService;

    public DoctorScheduleService(
        AppointmentDbContext context,
        IPatientService patientService,
        IMedicalRecordsService medicalRecordsService)
    {
        _context = context;
        _patientService = patientService;
        _medicalRecordsService = medicalRecordsService;
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

        var scheduleEvents = new List<DoctorScheduleEventDto>();

        foreach (var appointment in appointments)
        {
            var scheduleEvent = await EnrichAppointmentWithPatientDataAsync(appointment, cancellationToken);
            scheduleEvents.Add(scheduleEvent);
        }

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

        return await EnrichAppointmentWithPatientDataAsync(appointment, cancellationToken);
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

    private async Task<DoctorScheduleEventDto> EnrichAppointmentWithPatientDataAsync(
        AppointmentService.Models.Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        // Fetch patient data
        var patient = await _patientService.GetPatientAsync(appointment.PatientId, cancellationToken);

        // Fetch medical records
        var medicalRecord = await _medicalRecordsService.GetMedicalRecordAsync(appointment.PatientId, cancellationToken);

        return new DoctorScheduleEventDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown Patient",
            PatientAge = patient?.Age ?? 0,
            PatientPhone = patient?.PhoneNumber ?? "Not Available",
            PatientEmail = patient?.Email,
            AppointmentType = appointment.AppointmentType ?? "General",
            Date = appointment.ScheduledAt.ToString("yyyy-MM-dd"),
            Time = appointment.ScheduledAt.ToString("HH:mm"),
            Duration = (int)(appointment.ScheduledEndAt - appointment.ScheduledAt).TotalMinutes,
            Status = appointment.Status.ToLower(),
            ChiefComplaint = appointment.ChiefComplaint,
            Notes = appointment.Notes,
            MedicalHistory = medicalRecord?.MedicalHistory ?? new List<string>(),
            Allergies = medicalRecord?.Allergies ?? new List<string>(),
            CurrentMedications = medicalRecord?.CurrentMedications ?? new List<string>()
        };
    }
}
