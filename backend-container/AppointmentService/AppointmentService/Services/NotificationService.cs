using AppointmentService.Data;
using AppointmentService.Models;

namespace AppointmentService.Services;

public class NotificationService : INotificationService
{
    private readonly AppointmentDbContext _context;

    public NotificationService(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Recipient_User_Id = Guid.Parse(request.RecipientUserId),
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
        await _context.SaveChangesAsync();
    }
}
