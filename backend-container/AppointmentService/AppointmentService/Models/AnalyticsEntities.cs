using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentService.Models;

// These models represent the database schema structure for analytics
[Table("User")]
public class User
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Role_Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Schedule_Id { get; set; } = default!;
    
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public bool Is_Active { get; set; } = true;
}

[Table("User_Profile")]
public class UserProfile
{
    [Key]
    [MaxLength(36)]
    public string User_Id { get; set; } = default!;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [Required, MaxLength(255)]
    public string Email { get; set; } = default!;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(20)]
    public string? Gender { get; set; }
    
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}

[Table("Doctor")]
public class Doctor
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [MaxLength(100)]
    public string? License_Number { get; set; }
    
    public int? Years_Experience { get; set; }
    
    [MaxLength(2000)]
    public string? Biography { get; set; }
}

[Table("Patient")]
public class Patient
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string General_Doctor_Id { get; set; } = default!;
    
    [MaxLength(100)]
    public string? Medical_Record_Number { get; set; }
    
    [MaxLength(10)]
    public string? Blood_Type { get; set; }
}

[Table("Specialization")]
public class Specialization
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required, MaxLength(36)]
    public string Service_Id { get; set; } = default!;
    
    public bool Is_Active { get; set; } = true;
}

[Table("Doctor_Specialization")]
public class DoctorSpecialization
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Doctor_Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Specialization_Id { get; set; } = default!;
    
    public bool Is_Primary { get; set; } = false;
    public DateTime? Certified_Date { get; set; }
}

[Table("Schedule_Appointment")]
public class ScheduleAppointment
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Schedule_Id { get; set; } = default!;
    
    [MaxLength(36)]
    public string? Time_Slot_Id { get; set; }
    
    [Required]
    public DateTime Day { get; set; }
    
    public int? Duration_Minutes { get; set; }
    
    [MaxLength(255)]
    public string? Room { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Appointment_Type { get; set; } = "in-person";
    
    [Required, MaxLength(36)]
    public string Doctor_User_Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Patient_User_Id { get; set; } = default!;
    
    [MaxLength(36)]
    public string? Receptionist_User_Id { get; set; }
    
    [Required, MaxLength(36)]
    public string Schedule_Appointment_Status_Id { get; set; } = default!;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Total_Cost { get; set; }
    
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}

[Table("Schedule_Appointment_Status")]
public class ScheduleAppointmentStatus
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(7)]
    public string? Color_Code { get; set; }
}

[Table("Appointment_Payment")]
public class AppointmentPayment
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Amount { get; set; }
    
    [Required, MaxLength(10)]
    public string Currency { get; set; } = default!;
    
    [Required, MaxLength(32)]
    public string Status { get; set; } = default!;
    
    public DateTime? Paid_At { get; set; }
    public DateTime? Renewal_Date { get; set; }
    
    [Required, MaxLength(36)]
    public string Schedule_Appointment_Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Patient_Id { get; set; } = default!;
    
    [MaxLength(100)]
    public string? Payment_Method { get; set; }
    
    [MaxLength(200)]
    public string? Transaction_Id { get; set; }
}

[Table("Rate")]
public class Rate
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    public byte? Rate_Value { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required, MaxLength(36)]
    public string Patient_User_Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Doctor_User_Id { get; set; } = default!;
    
    [MaxLength(36)]
    public string? Appointment_Id { get; set; }
    
    public DateTime Rated_At { get; set; }
    public bool Is_Anonymous { get; set; } = false;
}

[Table("Notification")]
public class Notification
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    
    [Required, MaxLength(36)]
    public string Recipient_User_Id { get; set; } = default!;
    
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
