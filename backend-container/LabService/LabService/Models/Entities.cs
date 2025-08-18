using System.ComponentModel.DataAnnotations;

namespace LabService.Models;

public class LabOrder
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string OrderingDoctorId { get; set; } = default!;
    [MaxLength(36)]
    public string? MedicalRecordId { get; set; }
    [Required]
    public DateTime OrderedDate { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = "Ordered"; // Ordered, Collected, InProgress, Completed, Cancelled
    [MaxLength(500)]
    public string? ClinicalNotes { get; set; }
    [MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // Urgent, High, Normal, Low
    public DateTime? CollectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LabTest
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string LabOrderId { get; set; } = default!;
    [Required, MaxLength(20)]
    public string LoincCode { get; set; } = default!; // Reference to LOINC catalog
    [Required, MaxLength(200)]
    public string TestName { get; set; } = default!;
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    [MaxLength(500)]
    public string? Instructions { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LabResult
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string LabTestId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string PatientId { get; set; } = default!;
    [MaxLength(500)]
    public string? Value { get; set; }
    [MaxLength(100)]
    public string? Unit { get; set; }
    [MaxLength(200)]
    public string? ReferenceRange { get; set; }
    [MaxLength(50)]
    public string? Flag { get; set; } // Normal, High, Low, Critical
    [MaxLength(1000)]
    public string? Comments { get; set; }
    [Required]
    public DateTime ResultDate { get; set; }
    [MaxLength(36)]
    public string? ReviewedByDoctorId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(50)]
    public string ReviewStatus { get; set; } = "Pending"; // Pending, Reviewed, Acknowledged
    public DateTime CreatedAt { get; set; }
}

public class LabResultReview
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(36)]
    public string LabResultId { get; set; } = default!;
    [Required, MaxLength(36)]
    public string ReviewedByDoctorId { get; set; } = default!;
    [Required]
    public DateTime ReviewedAt { get; set; }
    [MaxLength(50)]
    public string ReviewStatus { get; set; } = default!; // Reviewed, RequiresFollowUp, Critical
    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }
    [MaxLength(1000)]
    public string? Recommendations { get; set; }
    public DateTime CreatedAt { get; set; }
}
