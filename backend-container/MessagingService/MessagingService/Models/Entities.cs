using System.ComponentModel.DataAnnotations;

namespace MessagingService.Models;

public class Message
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string SenderId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string RecipientId { get; set; } = default!;
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
    [MaxLength(36)]
    public string? RelatedEntityId { get; set; } // Reference to appointment, medical record, etc.
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; } // "Appointment", "MedicalRecord", etc.
    public DateTime CreatedAt { get; set; }
}

public class MessageThread
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(200)]
    public string Subject { get; set; } = default!;
    [Required, MaxLength(36)]
    public string InitiatorId { get; set; } = default!;
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
}

public class ThreadParticipant
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string ThreadId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string UserId { get; set; } = default!;
    [Required]
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
}

public class ThreadMessage
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string ThreadId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string SenderId { get; set; } = default!;
    [Required, MaxLength(2000)]
    public string Content { get; set; } = default!;
    [Required]
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageReceipt
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string MessageId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string UserId { get; set; } = default!;
    [Required]
    public DateTime ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
