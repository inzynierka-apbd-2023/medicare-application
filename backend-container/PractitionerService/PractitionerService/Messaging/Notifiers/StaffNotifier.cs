using MassTransit;
using Medicare.Messaging.Contracts;

namespace PractitionerService.Messaging.Notifiers;

public class StaffNotifier : IStaffNotifier
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<StaffNotifier> _logger;

    public StaffNotifier(IPublishEndpoint publishEndpoint, ILogger<StaffNotifier> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task NotifyDoctorArchived(Guid doctorId, Guid? userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing DoctorArchived for Doctor {DoctorId}", doctorId);

        await _publishEndpoint.Publish<IDoctorArchived>(new
        {
            DoctorId = doctorId,
            DoctorUserId = userId,
            OccurredAt = DateTime.UtcNow
        }, cancellationToken);
    }
}
