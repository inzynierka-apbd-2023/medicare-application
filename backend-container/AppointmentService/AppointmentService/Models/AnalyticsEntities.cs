using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentService.Models;

[Table("User")]
public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid Role_Id { get; set; }
    
    [Required]
    public Guid Schedule_Id { get; set; }
    
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public bool Is_Active { get; set; } = true;
}

[Table("User_Profile")]
public class UserProfile
{
    [Key]
    public Guid User_Id { get; set; }
    
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
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [MaxLength(100)]
    public string? License_Number { get; set; }
    
    public int? Years_Experience { get; set; }
    
    [MaxLength(2000)]
    public string? Biography { get; set; }
}

public class DoctorDirectory
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Specializations { get; set; }
    public bool IsActive { get; set; }
}

[Table("Patient")]
public class Patient
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid General_Doctor_Id { get; set; }
    
    [MaxLength(100)]
    public string? Medical_Record_Number { get; set; }
    
    [MaxLength(10)]
    public string? Blood_Type { get; set; }
}

[Table("Specialization")]
public class Specialization
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
}

[Table("Doctor_Specialization")]
public class DoctorSpecialization
{
    [Required]
    public Guid DoctorId { get; set; }
    
    [Required]
    public Guid SpecializationId { get; set; }
}

[Table("Schedule_Appointment")]
public class ScheduleAppointment
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid Schedule_Id { get; set; }
    
    public Guid? Time_Slot_Id { get; set; }
    
    [Required]
    public DateTime Day { get; set; }
    
    public int? Duration_Minutes { get; set; }
    
    [MaxLength(255)]
    public string? Room { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Appointment_Type { get; set; } = "in-person";
    
    [Required]
    public Guid Doctor_User_Id { get; set; }
    
    [Required]
    public Guid Patient_User_Id { get; set; }
    
    public Guid? Receptionist_User_Id { get; set; }
    
    [Required]
    public Guid Schedule_Appointment_Status_Id { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Total_Cost { get; set; }
    
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}

[Table("Schedule_Appointment_Status")]
public class ScheduleAppointmentStatus
{
    [Key]
    public Guid Id { get; set; }
    
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
    public Guid Id { get; set; }

    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public long AmountCents { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public Guid? PaymentIntentId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public DateTime ForDate { get; set; }
}

[Table("Rate")]
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

[Table("Notification")]
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
