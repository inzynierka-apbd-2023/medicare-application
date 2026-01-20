using AppointmentService.Data;
using AppointmentService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace AppointmentService.Services;

public class NotificationService : INotificationService
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public NotificationService(AppointmentDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task CreateNotificationAsync(CreateNotificationRequest request)
    {
        var recipientId = Guid.Parse(request.RecipientUserId);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Recipient_User_Id = recipientId,
            Description = request.Description,
            Type = request.Type,
            Creation_Date = DateTime.UtcNow,
            Source_Service = request.SourceService,
            Priority_Level = request.Priority,
            Action_Url = request.ActionUrl,
            Expires_At = request.ExpiresAt,
            Is_Read = false
        };

        _context.Notifications.Add(notification);
        
        await _publishEndpoint.Publish<INotificationCreated>(new
        {
            RecipientUserId = recipientId,
            request.Description,
            request.Type,
            request.SourceService,
            ActionUrl = request.ActionUrl,
            PriorityLevel = request.Priority,
            request.ExpiresAt
        });

        await _context.SaveChangesAsync();
    }
}
