using MassTransit;
using Medicare.Messaging.Contracts;
using NotificationService.Data;
using NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Consumers;

public class NotificationCreatedConsumer : IConsumer<INotificationCreated>
{
    private readonly ILogger<NotificationCreatedConsumer> _logger;
    private readonly NotificationsDbContext _db;

    public NotificationCreatedConsumer(ILogger<NotificationCreatedConsumer> logger, NotificationsDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Consume(ConsumeContext<INotificationCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[Notifications MT] Received notification: recipient={RecipientUserId}, type={Type}", msg.RecipientUserId, msg.Type);

        _db.Notifications.Add(new Notification
        {
            Recipient_User_Id = msg.RecipientUserId,
            Description = msg.Description,
            Type = msg.Type,
            Source_Service = msg.SourceService,
            Action_Url = msg.ActionUrl,
            Priority_Level = msg.PriorityLevel,
            Expires_At = msg.ExpiresAt,
            Creation_Date = DateTime.UtcNow,
            Is_Read = false
        });
        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
