using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Features.Patients.Consumers;

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
        var msg = context.Message;
        var idempotencyKey = context.MessageId?.ToString() ?? $"{msg.UserId}:{msg.OccurredAtUtc:O}";

        _logger.LogInformation("Processing UserRegistered for UserId: {UserId}", msg.UserId);

        var patient = await _db.Patients.SingleOrDefaultAsync(p => p.UserId == msg.UserId, context.CancellationToken);
        if (patient == null)
        {
            patient = new Patient
            {
                UserId = msg.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Patients.Add(patient);
            
            try 
            {
                await _db.SaveChangesAsync(context.CancellationToken);
            }
            catch (DbUpdateException)
            {
                // Concurrency handling - reload
                _db.ChangeTracker.Clear();
                patient = await _db.Patients.SingleAsync(p => p.UserId == msg.UserId, context.CancellationToken);
            }
        }

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

            try
            {
                await _db.SaveChangesAsync(context.CancellationToken);
            }
            catch (DbUpdateException)
            {
                // Already processed
                _logger.LogInformation("Duplicate PatientStatus prevented for UserId: {UserId}", msg.UserId);
            }
        }
    }
}
