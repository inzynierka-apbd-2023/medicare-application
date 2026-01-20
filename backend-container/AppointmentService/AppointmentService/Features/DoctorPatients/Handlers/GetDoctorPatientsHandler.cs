using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorPatients.DTOs;
using AppointmentService.Features.DoctorPatients.Queries;
using MassTransit;

namespace AppointmentService.Features.DoctorPatients.Handlers;

public class GetDoctorPatientsHandler : IRequestHandler<GetDoctorPatientsQuery, DoctorPatientsResponse>
{
    private readonly AppointmentDbContext _context;
    private readonly IRequestClient<Medicare.Messaging.Contracts.IGetPatients> _client;

    public GetDoctorPatientsHandler(AppointmentDbContext context, IRequestClient<Medicare.Messaging.Contracts.IGetPatients> client)
    {
        _context = context;
        _client = client;
    }

    public async Task<DoctorPatientsResponse> Handle(
        GetDoctorPatientsQuery request,
        CancellationToken cancellationToken)
    {
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

        var patientIds = patientAppointments.Select(p => p.PatientId).ToList();

        Dictionary<Guid, Medicare.Messaging.Contracts.IPatientProfile> userProfiles = new();

        var response = await _client.GetResponse<Medicare.Messaging.Contracts.IPatientProfiles>(new { PatientIds = patientIds }, cancellationToken);
        userProfiles = response.Message.Profiles.ToDictionary(p => p.PatientId);

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
