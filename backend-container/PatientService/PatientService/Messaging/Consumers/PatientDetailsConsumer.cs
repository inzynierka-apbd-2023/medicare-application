using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Messaging.Consumers;

public class PatientDetailsConsumer : IConsumer<IGetPatient>
{
    private readonly PatientDbContext _db;

    public PatientDetailsConsumer(PatientDbContext db)
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

        await context.RespondAsync<IPatientProfile>(new
        {
            profile.PatientId,
            FirstName = profile.FirstName ?? "",
            LastName = profile.LastName ?? "",
            Email = profile.Email ?? "",
            Phone = profile.Phone ?? "",
            DateOfBirth = profile.DateOfBirth,
            // Add other properties if available in PatientOverview
            Gender = (string?)null,
            AddressLine1 = (string?)null,
            AddressLine2 = (string?)null,
            City = (string?)null,
            State = (string?)null,
            ZipCode = (string?)null,
            Country = (string?)null
        });
    }
}
