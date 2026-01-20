using MediatR;
using PatientService.Models;
using PatientService.Data;
using Microsoft.EntityFrameworkCore;

namespace PatientService.Features.Patients.Queries.GetPatient;

public record GetPatientQuery(Guid Id) : IRequest<PatientProfileDto?>;

public class GetPatientHandler : IRequestHandler<GetPatientQuery, PatientProfileDto?>
{
    private readonly PatientDbContext _db;
    public GetPatientHandler(PatientDbContext db) => _db = db;

    public async Task<PatientProfileDto?> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient == null) return null;

        var overview = await _db.Set<PatientOverview>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.PatientId == patient.Id, cancellationToken);

        var contacts = await _db.EmergencyContacts
            .AsNoTracking()
            .Where(c => c.PatientId == patient.Id)
            .Select(c => new EmergencyContactDto(c.Name, c.Relation, c.Phone))
            .ToListAsync(cancellationToken);

        var insurance = await _db.Insurances
            .AsNoTracking()
            .Where(i => i.PatientId == patient.Id)
            .Select(i => new InsuranceDto(i.Provider, i.PolicyNumber, i.ValidFrom, i.ValidTo))
            .ToListAsync(cancellationToken);

        var fullName = overview != null ? $"{overview.FirstName} {overview.LastName}".Trim() : "Unknown";
        if (string.IsNullOrEmpty(fullName)) fullName = "Unknown";

        return new PatientProfileDto(
            patient.Id,
            patient.UserId,
            patient.PrimaryDoctorId,
            fullName,
            overview?.Email ?? "",
            overview?.Phone ?? "",
            overview != null ? string.Join(", ", new[] { overview.AddressLine1, overview.City, overview.Country }.Where(s => !string.IsNullOrWhiteSpace(s))) : "",
            overview?.DateOfBirth,
            overview?.Gender ?? "Unknown",
            contacts,
            insurance
        );
    }
}
