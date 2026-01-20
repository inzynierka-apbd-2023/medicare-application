using System.ComponentModel.DataAnnotations;

namespace PatientService.Models;

public class Patient
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; } // reference by ID to users.User
    public Guid? PrimaryDoctorId { get; set; } // reference by ID to practitioner.Doctor
    [MaxLength(10)]
    public string? BloodType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmergencyContact
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid PatientId { get; set; }
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
    public Guid Id { get; set; }
    [Required]
    public Guid PatientId { get; set; }
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
    public Guid Id { get; set; }
    [Required]
    public Guid PatientId { get; set; }
    [Required, MaxLength(50)]
    public string Status { get; set; } = default!; // e.g., Active, Inactive, Suspended
    public DateTime EffectiveAt { get; set; }
    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
}

// Projection read model
public class PatientOverview
{
    public Guid PatientId { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? CurrentStatus { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
