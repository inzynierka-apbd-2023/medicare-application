using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorPatients.DTOs;
using AppointmentService.Features.DoctorPatients.Queries;

namespace AppointmentService.Features.DoctorPatients.Handlers;

/// <summary>
/// Handler for getting all patients that have had appointments with a specific doctor.
/// Queries appointments grouped by patient, enriched with user profile data.
/// </summary>
public class GetDoctorPatientsHandler : IRequestHandler<GetDoctorPatientsQuery, DoctorPatientsResponse>
{
    private readonly AppointmentDbContext _context;

    public GetDoctorPatientsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorPatientsResponse> Handle(
        GetDoctorPatientsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all appointments for this doctor grouped by patient
        var patientAppointments = await _context.Appointments
            .Where(a => a.DoctorId == request.DoctorId)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                VisitCount = g.Count(),
                LastVisit = g.Max(a => a.ScheduledAt),
                LatestNotes = g.OrderByDescending(a => a.ScheduledAt).FirstOrDefault()!.Notes
            })
            .ToListAsync(cancellationToken);

        if (patientAppointments.Count == 0)
        {
            return new DoctorPatientsResponse { Patients = new List<DoctorPatientDto>(), TotalCount = 0 };
        }

        // Get patient IDs for profile lookup
        var patientIds = patientAppointments.Select(p => p.PatientId).ToList();

        // Fetch user profiles from the user schema (read-only)
        var userProfiles = await _context.UserProfiles
            .Where(up => patientIds.Contains(up.User_Id))
            .ToDictionaryAsync(up => up.User_Id, cancellationToken);

        // Map to DTOs
        var patients = patientAppointments.Select(pa =>
        {
            var profile = userProfiles.GetValueOrDefault(pa.PatientId);
            var age = profile?.DateOfBirth != null
                ? CalculateAge(profile.DateOfBirth.Value)
                : 0;

            return new DoctorPatientDto
            {
                Id = pa.PatientId,
                Name = profile != null
                    ? $"{profile.FirstName ?? ""} {profile.LastName ?? ""}".Trim()
                    : "Unknown Patient",
                Age = age,
                Gender = profile?.Gender ?? "Unknown",
                LastVisit = pa.LastVisit,
                Visits = pa.VisitCount,
                Notes = pa.LatestNotes ?? "",
                Email = profile?.Email,
                Phone = profile?.Phone
            };
        })
        .OrderBy(p => p.Name)
        .ToList();

        return new DoctorPatientsResponse
        {
            Patients = patients,
            TotalCount = patients.Count
        };
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
