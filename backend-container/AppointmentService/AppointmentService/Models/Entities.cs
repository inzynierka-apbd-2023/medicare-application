using System.ComponentModel.DataAnnotations;

namespace AppointmentService.Models;

public class Appointment
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid PatientId { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    [Required]
    public DateTime ScheduledAt { get; set; }
    [Required]
    public DateTime ScheduledEndAt { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, InProgress, Completed, Cancelled
    [MaxLength(100)]
    public string? AppointmentType { get; set; }
    [MaxLength(1000)]
    public string? ChiefComplaint { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
    public Guid? RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Tracks when an upcoming notification was sent to avoid duplicate MQ messages
    public DateTime? UpcomingNotificationSentAt { get; set; }
    // Tracks when the 30-minute reminder was sent
    public DateTime? ThirtyMinNotificationSentAt { get; set; }
}

public class AppointmentSlot
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required]
    public bool IsAvailable { get; set; } = true;
    public Guid? AppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Schedule
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
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
    public Guid Id { get; set; }
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
