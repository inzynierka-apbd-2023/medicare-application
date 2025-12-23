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

        // Soft delete logic: Set status to Inactive
        // Check if status exists, insert new status
        
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

        // Alternatively, if we wanted HARD delete, we would remove from _db.Patients
        // But functionality requirements usually imply soft delete for medical records unless specified "Removing from base" literally.
        // functionality.md says "Usuwanie pacjentów z bazy" (Deleting patients from database). 
        // Given I'm admin/reception, often this means hard delete or soft delete.
        // Let's stick to Soft Delete (Inactive) as it's safer, BUT check if I should do strict delete.
        // The implementation_plan mentioned "Delete endpoint". 
        // Let's assume Soft Delete via Status is the preferred "business" delete. 
        // Wait, current frontend mock does: `mockPatients[patientIndex].isActive = false;`. So yes, soft delete.

        return true;
    }
}
