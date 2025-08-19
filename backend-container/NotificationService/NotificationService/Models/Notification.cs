namespace NotificationService.Models;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Recipient_User_Id { get; set; } = default!; // FK to Users or auth directory
    public string? Description { get; set; }
    public byte Type { get; set; } // tinyint
    public DateTime Creation_Date { get; set; } = DateTime.UtcNow;
    public string? Source_Service { get; set; }
    public bool? Is_Read { get; set; } // nullable per schema
    public string? Action_Url { get; set; }
    public string? Priority_Level { get; set; }
    public DateTime? Expires_At { get; set; }
}
