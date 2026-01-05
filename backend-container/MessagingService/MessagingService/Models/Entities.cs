using System.ComponentModel.DataAnnotations;

namespace MessagingService.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid SenderId { get; set; }
    [Required]
    public Guid RecipientId { get; set; }
    [Required, MaxLength(200)]
    public string Subject { get; set; } = default!;
    [Required, MaxLength(2000)]
    public string Content { get; set; } = default!;
    [MaxLength(50)]
    public string MessageType { get; set; } = "General"; // General, Appointment, Medical, System
    [MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    [Required]
    public bool IsRead { get; set; } = false;
    [Required]
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid? RelatedEntityId { get; set; } // Reference to appointment, medical record, etc.
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; } // "Appointment", "MedicalRecord", etc.
    public DateTime CreatedAt { get; set; }

    [MaxLength(100)]
    public string? SenderName { get; set; }
    [MaxLength(100)]
    public string? RecipientName { get; set; }
}

public class MessageThread
{
    [Key]
    public Guid Id { get; set; }
    [Required, MaxLength(200)]
    public string Subject { get; set; } = default!;
    [Required]
    public Guid InitiatorId { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
}

public class ThreadParticipant
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid ThreadId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
}

public class ThreadMessage
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid ThreadId { get; set; }
    [Required]
    public Guid SenderId { get; set; }
    [Required, MaxLength(2000)]
    public string Content { get; set; } = default!;
    [Required]
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageReceipt
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid MessageId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public DateTime ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Denormalized table tracking which doctors a patient can message.
/// Populated via RabbitMQ events when appointments are created.
/// </summary>
public class PatientDoctorContact
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>Patient's User ID</summary>
    [Required]
    public Guid PatientUserId { get; set; }
    
    /// <summary>Doctor's User ID (used for messaging)</summary>
    [Required]
    public Guid DoctorUserId { get; set; }
    
    /// <summary>Doctor's profile ID in PractitionerService</summary>
    public Guid? DoctorProfileId { get; set; }
    
    [MaxLength(200)]
    public string? DoctorName { get; set; }
    
    [MaxLength(200)]
    public string? DoctorSpecialization { get; set; }
    
    /// <summary>When the first appointment was created</summary>
    public DateTime FirstContactAt { get; set; }
    
    /// <summary>When the last appointment was created</summary>
    public DateTime LastContactAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

