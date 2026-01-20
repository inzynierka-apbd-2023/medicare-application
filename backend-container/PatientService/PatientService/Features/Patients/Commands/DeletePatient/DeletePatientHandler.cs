using MediatR;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;

namespace PatientService.Features.Patients.Commands.DeletePatient;

public class DeletePatientHandler : IRequestHandler<DeletePatientCommand, bool>
{
    private readonly PatientDbContext _db;

    public DeletePatientHandler(PatientDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);
        if (patient == null) return false;

        var currentStatus = await _db.PatientStatuses
            .Where(s => s.PatientId == request.PatientId)
            .OrderByDescending(s => s.EffectiveAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentStatus == null || currentStatus.Status != "Inactive")
        {
            _db.PatientStatuses.Add(new Models.PatientStatus
            {
                PatientId = request.PatientId,
                Status = "Inactive",
                EffectiveAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
        return true;
    }
}
