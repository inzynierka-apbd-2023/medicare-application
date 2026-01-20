using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentService.Models;

[Table("Notification", Schema = "appointment")]
public class Notification
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid Recipient_User_Id { get; set; }
    
    [Required, MaxLength(255)]
    public string Description { get; set; } = default!;
    
    [Required]
    public byte Type { get; set; }
    
    [Required]
    public DateTime Creation_Date { get; set; }
    
    [Required, MaxLength(64)]
    public string Source_Service { get; set; } = default!;
    
    public bool Is_Read { get; set; } = false;
    
    [MaxLength(500)]
    public string? Action_Url { get; set; }
    
    [MaxLength(20)]
    public string Priority_Level { get; set; } = "Normal";
    
    public DateTime? Expires_At { get; set; }
}
