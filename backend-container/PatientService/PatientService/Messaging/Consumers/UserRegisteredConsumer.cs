using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Messaging.Consumers;

public class UserRegisteredConsumer : IConsumer<IUserRegistered>
{
    private readonly PatientDbContext _db;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(PatientDbContext db, ILogger<UserRegisteredConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IUserRegistered> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Processing UserRegistered for {UserId}", evt.UserId);

        var patient = await _db.Patients.SingleOrDefaultAsync(p => p.UserId == evt.UserId, context.CancellationToken);
        if (patient == null)
        {
            patient = new Patient
            {
                UserId = evt.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        var idempotencyKey = context.MessageId?.ToString() ?? $"{evt.UserId}:{evt.OccurredAtUtc:O}";
        var statusExists = await _db.PatientStatuses.AnyAsync(s => s.IdempotencyKey == idempotencyKey, context.CancellationToken);
        if (!statusExists)
        {
            _db.PatientStatuses.Add(new PatientStatus
            {
                Status = "Active",
                EffectiveAt = DateTime.UtcNow,
                PatientId = patient.Id,
                IdempotencyKey = idempotencyKey
            });
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}
