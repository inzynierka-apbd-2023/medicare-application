using AppointmentService.Data;
using AppointmentService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace AppointmentService.Messaging.Notifiers;

public interface ISystemNotificationNotifier
{
    Task NotifySystemEvent(string recipientId, string description, int type, string sourceService, string priority, string? actionUrl = null, DateTime? expiresAt = null);
}

public class SystemNotificationNotifier : ISystemNotificationNotifier
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SystemNotificationNotifier(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task NotifySystemEvent(string recipientId, string description, int type, string sourceService, string priority, string? actionUrl = null, DateTime? expiresAt = null)
    {
        var recipientGuid = Guid.Parse(recipientId);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Recipient_User_Id = recipientGuid,
            Description = description,
            Type = (byte)type,
            Creation_Date = DateTime.UtcNow,
            Source_Service = sourceService,
            Priority_Level = priority,
            Action_Url = actionUrl,
            Expires_At = expiresAt,
            Is_Read = false
        };

        _context.Notifications.Add(notification);
        
        await _publishEndpoint.Publish<INotificationCreated>(new
        {
            RecipientUserId = recipientGuid,
            Description = description,
            Type = type,
            SourceService = sourceService,
            ActionUrl = actionUrl,
            PriorityLevel = priority,
            ExpiresAt = expiresAt
        });

        await _context.SaveChangesAsync();
    }
}
