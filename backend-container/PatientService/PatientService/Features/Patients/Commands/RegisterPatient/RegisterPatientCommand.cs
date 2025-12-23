using MediatR;
using PatientService.Models;
using PatientService.Data;

namespace PatientService.Features.Patients.Commands.RegisterPatient;

public record RegisterPatientCommand(Guid UserId, string? PrimaryDoctorId) : IRequest<Patient?>;

public class RegisterPatientHandler : IRequestHandler<RegisterPatientCommand, Patient?>
{
    private readonly PatientDbContext _db;
    public RegisterPatientHandler(PatientDbContext db) => _db = db;

    public async Task<Patient?> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        // Validation logic moved here
        var userIdStr = request.UserId.ToString();
        var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_db.Patients, p => p.UserId == userIdStr, cancellationToken);
        if (exists) return null; // Or throw exception, but returning null fits existing controller logic of check

        var patient = new Patient
        {
            UserId = userIdStr,
            PrimaryDoctorId = request.PrimaryDoctorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Patients.Add(patient);
        _db.PatientStatuses.Add(new PatientStatus
        {
            PatientId = patient.Id,
            Status = "Active",
            EffectiveAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        
        return patient;
    }
}
