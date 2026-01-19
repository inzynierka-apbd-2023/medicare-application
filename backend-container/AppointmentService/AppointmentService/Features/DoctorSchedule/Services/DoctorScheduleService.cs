using System.Linq;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Models;

namespace AppointmentService.Features.DoctorSchedule.Services;

public class DoctorScheduleService : IDoctorScheduleService
{
    private const string UnknownPatient = "Unknown Patient";
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
        var appointments = await FetchAppointmentsAsync(doctorId, startDate, endDate, status, cancellationToken);
        var profiles = await FetchPatientProfilesAsync(appointments, cancellationToken);

        UpdateOverdueStatuses(appointments);

        var scheduleEvents = appointments
            .Select(a => MapToScheduleEvent(a, profiles.GetValueOrDefault(a.PatientId)))
            .ToList();

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

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.User_Id == appointment.PatientId, cancellationToken);

        return MapToScheduleEvent(appointment, profile);
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

    private async Task<List<Appointment>> FetchAppointmentsAsync(
        Guid doctorId,
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        CancellationToken cancellationToken)
    {
        var query = _context.Appointments.Where(a => a.DoctorId == doctorId);

        if (startDate.HasValue)
            query = query.Where(a => a.ScheduledAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.ScheduledAt <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        return await query.OrderBy(a => a.ScheduledAt).ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, UserProfile>> FetchPatientProfilesAsync(
        List<Appointment> appointments,
        CancellationToken cancellationToken)
    {
        var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();

        return await _context.UserProfiles
            .Where(p => patientIds.Contains(p.User_Id))
            .ToDictionaryAsync(p => p.User_Id, cancellationToken);
    }

    private static void UpdateOverdueStatuses(List<Appointment> appointments)
    {
        var now = DateTime.Now;

        foreach (var appointment in appointments)
        {
            if (IsOverdue(appointment, now))
            {
                appointment.Status = "Overdue";
            }
        }
    }

    private static bool IsOverdue(Appointment appointment, DateTime now) =>
        (appointment.Status == "Scheduled" || appointment.Status == "Confirmed") && appointment.ScheduledEndAt < now;

    private static DoctorScheduleEventDto MapToScheduleEvent(Appointment appointment, UserProfile? profile)
    {
        var (patientName, patientAge, patientPhone, patientEmail) = ExtractPatientDetails(profile);

        return new DoctorScheduleEventDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patientName,
            PatientAge = patientAge,
            PatientPhone = patientPhone,
            PatientEmail = patientEmail,
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

    private static (string Name, int Age, string Phone, string? Email) ExtractPatientDetails(UserProfile? profile)
    {
        if (profile == null)
            return (UnknownPatient, 0, "", null);

        var name = BuildPatientName(profile);
        var age = CalculateAge(profile.DateOfBirth);

        return (name, age, profile.Phone ?? "", profile.Email);
    }

    private static string BuildPatientName(UserProfile profile)
    {
        var name = $"{profile.FirstName} {profile.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? UnknownPatient : name;
    }

    private static int CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return 0;

        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;

        if (dateOfBirth.Value.Date > today.AddYears(-age))
            age--;

        return age;
    }
}
