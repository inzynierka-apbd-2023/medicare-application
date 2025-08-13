using System.ComponentModel.DataAnnotations;

namespace PatientService.Models;

public class Patient
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string UserId { get; set; } = default!; // reference by ID to users.User
    [MaxLength(36)]
    public string? PrimaryDoctorId { get; set; } // reference by ID to practitioner.Doctor
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmergencyContact
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    [MaxLength(100)]
    public string? Relation { get; set; }
    [MaxLength(100)]
    public string? Phone { get; set; }
}

public class Insurance
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [MaxLength(200)]
    public string? Provider { get; set; }
    [MaxLength(100)]
    public string? PolicyNumber { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class PatientStatus
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [Required, MaxLength(50)]
    public string Status { get; set; } = default!; // e.g., Active, Inactive, Suspended
    public DateTime EffectiveAt { get; set; }
}

// Projection read model
public class PatientOverview
{
    public string PatientId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CurrentStatus { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
