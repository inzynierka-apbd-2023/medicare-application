using MediatR;
using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Data;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Features.MedicalRecords.Queries.GetPatientHistory;

public record GetPatientHistoryQuery(Guid PatientId) : IRequest<PatientHistoryDto>;

public class GetPatientHistoryHandler : IRequestHandler<GetPatientHistoryQuery, PatientHistoryDto>
{
    private readonly MedicalRecordsDbContext _db;
    public GetPatientHistoryHandler(MedicalRecordsDbContext db) => _db = db;

    public async Task<PatientHistoryDto> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
    {
        var patientId = request.PatientId;

        // 1. Fetch Medical Records
        var records = await _db.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.VisitDate)
            .ToListAsync(cancellationToken);

        var recordIds = records.Select(r => r.Id).ToList();

        // 2. Fetch Diagnoses (via MedicalRecordId)
        var conditions = await _db.Diagnoses
            .AsNoTracking()
            .Where(d => recordIds.Contains(d.MedicalRecordId))
            .ToListAsync(cancellationToken);

        // 3. Fetch Prescriptions (By PatientId directly as per schema)
        var medications = await _db.Prescriptions
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync(cancellationToken);

        // 4. Fetch Vitals (By PatientId directly)
        var vitals = await _db.VitalSigns
            .AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.MeasuredAt)
            .ToListAsync(cancellationToken);

        return new PatientHistoryDto
        {
            PatientId = patientId,
            Records = records,
            Conditions = conditions,
            Medications = medications,
            Vitals = vitals
        };
    }
}
