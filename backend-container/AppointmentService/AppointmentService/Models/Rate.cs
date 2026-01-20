using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentService.Models;

[Table("Rate", Schema = "appointment")]
public class Rate
{
    [Key]
    public Guid Id { get; set; }
    
    public byte? Rate_Value { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid Patient_User_Id { get; set; }
    
    [Required]
    public Guid Doctor_User_Id { get; set; }
    
    public Guid? Appointment_Id { get; set; }
    
    public DateTime Rated_At { get; set; }
    public bool Is_Anonymous { get; set; } = false;
}
