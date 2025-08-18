namespace AppointmentService.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(CreateNotificationRequest request);
}

public class CreateNotificationRequest
{
    public string RecipientUserId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte Type { get; set; }
    public string SourceService { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public string? ActionUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
