using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Features.Patients.Consumers;

public class GetPatientsConsumer : IConsumer<IGetPatients>
{
    private readonly PatientDbContext _db;

    public GetPatientsConsumer(PatientDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<IGetPatients> context)
    {
        var patientIds = context.Message.PatientIds;

        if (patientIds == null || !patientIds.Any())
        {
            await context.RespondAsync<IPatientProfiles>(new { Profiles = new List<object>() });
            return;
        }

        var profiles = await _db.Set<PatientOverview>()
            .Where(p => patientIds.Contains(p.PatientId))
            .AsNoTracking()
            .ToListAsync(context.CancellationToken);

        await context.RespondAsync<IPatientProfiles>(new
        {
            Profiles = profiles.Select(p => new
            {
                p.PatientId,
                UserId = p.UserId,
                FirstName = p.FirstName ?? "",
                LastName = p.LastName ?? "",
                Email = p.Email ?? "",
                Phone = p.Phone ?? "",
                p.DateOfBirth,
                p.Gender,
                p.AddressLine1,
                p.AddressLine2,
                p.City,
                p.State,
                p.ZipCode,
                p.Country
            }).ToList()
        });
    }
}
