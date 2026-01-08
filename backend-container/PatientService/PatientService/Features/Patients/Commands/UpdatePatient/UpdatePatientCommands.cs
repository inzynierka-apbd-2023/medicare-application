using MediatR;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Features.Patients.Commands.UpdatePatient;

public record ChangePatientStatusCommand(Guid Id, string Status) : IRequest<bool>;

public class ChangePatientStatusHandler : IRequestHandler<ChangePatientStatusCommand, bool>
{
    private readonly PatientDbContext _db;
    public ChangePatientStatusHandler(PatientDbContext db) => _db = db;

    public async Task<bool> Handle(ChangePatientStatusCommand request, CancellationToken cancellationToken)
    {
        var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_db.Patients, p => p.Id == request.Id, cancellationToken);
        if (!exists) return false;

        _db.PatientStatuses.Add(new PatientStatus
        {
            PatientId = request.Id,
            Status = request.Status,
            EffectiveAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record SetEmergencyContactsCommand(Guid Id, List<EmergencyContactDto> Contacts) : IRequest<bool>;


public class SetEmergencyContactsHandler : IRequestHandler<SetEmergencyContactsCommand, bool>
{
    private readonly PatientDbContext _db;
    public SetEmergencyContactsHandler(PatientDbContext db) => _db = db;

    public async Task<bool> Handle(SetEmergencyContactsCommand request, CancellationToken cancellationToken)
    {
        var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_db.Patients, p => p.Id == request.Id, cancellationToken);
        if (!exists) return false;

        var current = _db.EmergencyContacts.Where(c => c.PatientId == request.Id);
        _db.EmergencyContacts.RemoveRange(current);
        _db.EmergencyContacts.AddRange(request.Contacts.Select(c => new EmergencyContact
        {
            PatientId = request.Id,
            Name = c.Name,
            Relation = c.Relation,
            Phone = c.Phone
        }));
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record UpdateInsuranceCommand(Guid Id, string? Provider, string? PolicyNumber, DateTime? ValidFrom, DateTime? ValidTo) : IRequest<bool>;

public class UpdateInsuranceHandler : IRequestHandler<UpdateInsuranceCommand, bool>
{
    private readonly PatientDbContext _db;
    public UpdateInsuranceHandler(PatientDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateInsuranceCommand request, CancellationToken cancellationToken)
    {
        var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_db.Patients, p => p.Id == request.Id, cancellationToken);
        if (!exists) return false;

        var existing = _db.Insurances.Where(i => i.PatientId == request.Id);
        _db.Insurances.RemoveRange(existing);
        _db.Insurances.Add(new Insurance
        {
            PatientId = request.Id,
            Provider = request.Provider,
            PolicyNumber = request.PolicyNumber,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        });
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record SetPrimaryDoctorCommand(Guid Id, Guid? DoctorId) : IRequest<bool>;

public class SetPrimaryDoctorHandler : IRequestHandler<SetPrimaryDoctorCommand, bool>
{
    private readonly PatientDbContext _db;
    public SetPrimaryDoctorHandler(PatientDbContext db) => _db = db;

    public async Task<bool> Handle(SetPrimaryDoctorCommand request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FindAsync(new object[] { request.Id }, cancellationToken);
        if (patient == null) return false;

        patient.PrimaryDoctorId = request.DoctorId;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

