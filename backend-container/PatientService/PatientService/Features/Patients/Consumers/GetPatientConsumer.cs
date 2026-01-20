using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Features.Patients.Consumers;

public class GetPatientConsumer : IConsumer<IGetPatient>
{
    private readonly PatientDbContext _db;

    public GetPatientConsumer(PatientDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<IGetPatient> context)
    {
        var patientId = context.Message.PatientId;
        var profile = await _db.Set<PatientOverview>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId || p.UserId == patientId, context.CancellationToken);

        if (profile == null)
        {
            throw new InvalidOperationException($"Patient not found: {patientId}");
        }

        // Also fetch Patient entity to get BloodType
        var patient = await _db.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profile.PatientId, context.CancellationToken);

        await context.RespondAsync<IPatientProfile>(new
        {
            profile.PatientId,
            FirstName = profile.FirstName ?? "",
            LastName = profile.LastName ?? "",
            Email = profile.Email ?? "",
            Phone = profile.Phone ?? "",
            profile.DateOfBirth,
            // Add other properties if available in PatientOverview
            Gender = profile.Gender,
            AddressLine1 = profile.AddressLine1,
            AddressLine2 = profile.AddressLine2,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            BloodType = patient?.BloodType
        });
    }
}
