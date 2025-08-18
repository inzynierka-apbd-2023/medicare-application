using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PractitionerService.Models;

public class Doctor
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string UserId { get; set; } = default!; // reference to users.User
    [MaxLength(500)]
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}

public class Receptionist
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string UserId { get; set; } = default!; // reference to users.User
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MedicalService
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class Specialization
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
}

// Mapping between Services and Specializations (many-to-many)
public class SpecializationService
{
    [MaxLength(36)]
    public string SpecializationId { get; set; } = default!;
    [MaxLength(36)]
    public string ServiceId { get; set; } = default!;
}

public class DoctorSpecialization
{
    [MaxLength(36)]
    public string DoctorId { get; set; } = default!;
    [MaxLength(36)]
    public string SpecializationId { get; set; } = default!;
}

public class DoctorSchedule
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string DoctorId { get; set; } = default!;
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
    public string DoctorId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Specializations { get; set; } // comma-separated
    public string? Services { get; set; } // comma-separated (reserved)
}
