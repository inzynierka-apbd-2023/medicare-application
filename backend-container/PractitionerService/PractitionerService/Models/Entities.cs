using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractitionerService.Models;

public class Doctor
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; } // reference to users.User
    [MaxLength(500)]
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}

public class Receptionist
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; } // reference to users.User
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MedicalService
{
    [Key]
    public Guid Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class Specialization
{
    [Key]
    public Guid Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
}

// Mapping between Services and Specializations (many-to-many)
public class SpecializationService
{
    public Guid SpecializationId { get; set; }
    public Guid ServiceId { get; set; }
}

public class DoctorSpecialization
{
    public Guid DoctorId { get; set; }
    public Guid SpecializationId { get; set; }
}

public class DoctorSchedule
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    [Range(0,6)]
    public int DayOfWeek { get; set; } // 0=Sunday
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Read model (projection)
public class DoctorDirectory
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Specializations { get; set; } // comma-separated
    public string? Services { get; set; } // comma-separated (reserved)
    public bool IsActive { get; set; }
}

// Async saga state for user->doctor creation via RabbitMQ
public class PendingDoctor
{
    [Key]
    public Guid Id { get; set; } // correlationId

    // Profile
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = default!;
    [Required, MaxLength(100)]
    public string LastName { get; set; } = default!;
    [Required, MaxLength(255)]
    public string Email { get; set; } = default!;
    [MaxLength(50)] public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(10)] public string? Gender { get; set; }
    [MaxLength(200)] public string? AddressLine1 { get; set; }
    [MaxLength(200)] public string? AddressLine2 { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }

    // Practitioner specific
    [MaxLength(500)] public string? Biography { get; set; }
    public string? SpecializationIdsCsv { get; set; }

    // Generated creds
    [MaxLength(60)] public string Username { get; set; } = default!;
    [MaxLength(100)] public string Password { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}

public class DoctorStatistics
{
    [Key]
    public Guid DoctorId { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int TotalRatingSum { get; set; }
    public int TotalRatingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

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
