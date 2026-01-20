using ArchiveService.Data;
using ArchiveService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace ArchiveService.Messaging.Consumers;

public class DoctorRemovalRequestedConsumer : IConsumer<IDoctorRemovalRequested>
{
    private readonly ArchiveDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DoctorRemovalRequestedConsumer> _logger;

    public DoctorRemovalRequestedConsumer(ArchiveDbContext db, IPublishEndpoint publishEndpoint, ILogger<DoctorRemovalRequestedConsumer> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IDoctorRemovalRequested> context)
    {
        var msg = context.Message;

        var existing = await _db.ArchivedDoctors.FindAsync(msg.DoctorId, context.CancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("Doctor {DoctorId} already archived", msg.DoctorId);
            return;
        }

        var archived = new ArchivedDoctor
        {
            DoctorId = msg.DoctorId,
            UserId = msg.DoctorUserId,
            FullName = msg.FullName ?? string.Empty,
            Email = msg.Email,
            Phone = msg.Phone,
            ArchivedAtUtc = DateTime.UtcNow,
            SnapshotJson = msg.SnapshotJson
        };

        _db.ArchivedDoctors.Add(archived);

        await _publishEndpoint.Publish<IDoctorArchived>(new
        {
            msg.DoctorId,
            msg.DoctorUserId,
            OccurredAt = DateTime.UtcNow,
            msg.FullName,
            msg.Email,
            msg.Phone,
            msg.SnapshotJson
        }, context.CancellationToken);

        await _db.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Archived doctor {DoctorId}", msg.DoctorId);
    }
}
