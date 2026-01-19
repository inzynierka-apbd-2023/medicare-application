using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Consumers;

public class PatientGenericProfileConsumer : IConsumer<IGetPatient>
{
    private readonly UsersDbContext _db;
    private readonly ILogger<PatientGenericProfileConsumer> _logger;

    public PatientGenericProfileConsumer(UsersDbContext db, ILogger<PatientGenericProfileConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetPatient> context)
    {
        var patientId = context.Message.PatientId;
        _logger.LogInformation("Processing GetPatient request for {PatientId}", patientId);

        var user = await _db.Users.FindAsync(new object[] { patientId }, context.CancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"Patient {patientId} not found");
        }

        await context.RespondAsync<IPatientProfile>(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.DateOfBirth,
            user.Gender,
            user.AddressLine1,
            user.AddressLine2,
            user.City,
            user.State,
            user.ZipCode,
            user.Country
        });
    }
}
