using System.ComponentModel.DataAnnotations;

namespace MedicalRecordsService.Models;

public class MedicalRecord
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid PatientId { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }
    [Required]
    public DateTime VisitDate { get; set; }
    [MaxLength(200)]
    public string? ChiefComplaint { get; set; }
    [MaxLength(1000)]
    public string? HistoryOfPresentIllness { get; set; }
    [MaxLength(1000)]
    public string? PhysicalExamination { get; set; }
    [MaxLength(1000)]
    public string? Assessment { get; set; }
    [MaxLength(1000)]
    public string? Plan { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Prescription
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid MedicalRecordId { get; set; }
    [Required]
    public Guid PatientId { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    [Required, MaxLength(200)]
    public string MedicationName { get; set; } = default!;
    [MaxLength(10)]
    public string? AtcCode { get; set; } // Reference to ATC catalog
    [Required, MaxLength(100)]
    public string Dosage { get; set; } = default!;
    [Required, MaxLength(100)]
    public string Frequency { get; set; } = default!;
    [Required]
    public int DurationDays { get; set; }
    [MaxLength(500)]
    public string? Instructions { get; set; }
    [Required]
    public DateTime PrescribedDate { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Diagnosis
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid MedicalRecordId { get; set; }
    [Required, MaxLength(10)]
    public string Icd10Code { get; set; } = default!; // Reference to ICD-10 catalog
    [Required, MaxLength(500)]
    public string Description { get; set; } = default!;
    [MaxLength(50)]
    public string Type { get; set; } = "Primary"; // Primary, Secondary
    [MaxLength(500)]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VitalSigns
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid MedicalRecordId { get; set; }
    [Required]
    public Guid PatientId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public decimal? Temperature { get; set; } // Celsius
    public int? SystolicBP { get; set; }
    public int? DiastolicBP { get; set; }
    public int? HeartRate { get; set; } // BPM
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturation { get; set; } // Percentage
    public decimal? Height { get; set; } // cm
    public decimal? Weight { get; set; } // kg
    [MaxLength(500)]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
