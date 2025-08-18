using System.ComponentModel.DataAnnotations;

namespace AppointmentService.Models;

public class Appointment
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string DoctorId { get; set; } = default!;
    [Required]
    public DateTime ScheduledAt { get; set; }
    [Required]
    public DateTime ScheduledEndAt { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, InProgress, Completed, Cancelled
    [MaxLength(100)]
    public string? AppointmentType { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
    [MaxLength(36)]
    public string? RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentSlot
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string DoctorId { get; set; } = default!;
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required]
    public bool IsAvailable { get; set; } = true;
    [MaxLength(36)]
    public string? AppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Schedule
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string DoctorId { get; set; } = default!;
    [Required]
    public DayOfWeek DayOfWeek { get; set; }
    [Required]
    public TimeOnly StartTime { get; set; }
    [Required]
    public TimeOnly EndTime { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentCategory
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Required]
    public int DurationMinutes { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
