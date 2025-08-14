using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentsService.Models;

public enum DocumentKind
{
    VisitNote = 1,
    Prescription = 2,
    Referral = 3,
    SickLeave = 4,
    LabResults = 5
}

[Table("Document", Schema = "documents")]
public class Document
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int Type { get; set; } // mirrors DocumentKind and Document_Type

    [MaxLength(36)]
    public string DocumentTypeId { get; set; } = default!;

    [MaxLength(36)]
    public string PatientId { get; set; } = default!;

    [MaxLength(36)]
    public string DoctorId { get; set; } = default!;

    [MaxLength(500)]
    public string? FilePath { get; set; }

    public long? FileSizeBytes { get; set; }

    public DocumentType? DocumentType { get; set; }

    public VisitDocument? VisitDocument { get; set; }
    public Prescription? Prescription { get; set; }
    public Referral? Referral { get; set; }
    public SickLeave? SickLeave { get; set; }
    public LabResults? LabResults { get; set; }

    public ICollection<DocumentAssignment> Assignments { get; set; } = new List<DocumentAssignment>();
}

[Table("Document_Type", Schema = "documents")]
public class DocumentType
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = default!; // VISIT_NOTE, PRESCRIPTION, etc.

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? TemplatePath { get; set; }
}

[Table("Visit_Document", Schema = "documents")]
public class VisitDocument
{
    [Key]
    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    public string? Symptoms { get; set; }
    public string? Findings { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }
    public string? VitalSignsJson { get; set; }
    public string? TreatmentPlan { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

[Table("Prescription", Schema = "documents")]
public class Prescription
{
    [Key]
    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    // Canonical medication name from catalog.atc (filled from AtcCode)
    [MaxLength(500)]
    public string Medication { get; set; } = default!;
    [MaxLength(200)]
    public string? Dosage { get; set; }
    [MaxLength(200)]
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    [MaxLength(1000)]
    public string? Instructions { get; set; }
    [MaxLength(200)]
    public string? PharmacyName { get; set; }
    [MaxLength(50)]
    public string? PharmacyPhone { get; set; }
    public int? RefillsRemaining { get; set; }

    // Link to MedicalCatalogService: catalog.atc.AtcCode; enforced by controller using HTTP to catalog API
    [MaxLength(20)]
    public string? AtcCode { get; set; }
    [MaxLength(500)]
    public string? AtcName { get; set; }
}

[Table("Referral", Schema = "documents")]
public class Referral
{
    [Key]
    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    [MaxLength(200)]
    public string? Speciality { get; set; }
    [MaxLength(200)]
    public string? ReferredTo { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    [MaxLength(1000)]
    public string? Reason { get; set; }
    [MaxLength(50)]
    public string? UrgencyLevel { get; set; }
}

[Table("Sick_Leave", Schema = "documents")]
public class SickLeave
{
    [Key]
    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DaysOff { get; set; }
    public DateTime? ReturnToWorkDate { get; set; }
    [MaxLength(500)]
    public string? WorkRestrictions { get; set; }
}

[Table("Lab_Results", Schema = "documents")]
public class LabResults
{
    [Key]
    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    [MaxLength(200)]
    public string? TestType { get; set; }
    public DateTime? TestDate { get; set; }
    [MaxLength(200)]
    public string? Laboratory { get; set; }
    [MaxLength(50)]
    public string? OverallStatus { get; set; }
    public string? Interpretation { get; set; }
    public string? ReferenceRanges { get; set; }
    [MaxLength(200)]
    public string? TechnicianName { get; set; }
    public string? DoctorComments { get; set; }

    public ICollection<LabTestResult> Results { get; set; } = new List<LabTestResult>();
}

[Table("Lab_Test_Result", Schema = "documents")]
public class LabTestResult
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(36)]
    public string LabResultsDocumentId { get; set; } = default!; // FK -> Lab_Results.Document_Id

    [MaxLength(36)]
    public string? LabTestTypeId { get; set; } // FK -> documents.Lab_Test_Type.Id (projection of LOINC)

    [MaxLength(20)]
    public string? LoincCode { get; set; } // direct code on the line for ingestion

    [MaxLength(200)]
    public string? ParameterName { get; set; }
    [MaxLength(200)]
    public string? Value { get; set; }
    public decimal? NumericValue { get; set; }
    [MaxLength(50)]
    public string? Unit { get; set; }
    [MaxLength(200)]
    public string? ReferenceRange { get; set; }
    [MaxLength(50)]
    public string? Status { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public bool? IsAbnormal { get; set; }

    public LabTestType? LabTestType { get; set; }
}

[Table("Documents_Assigned", Schema = "documents")]
public class DocumentAssignment
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(36)]
    public string DocumentId { get; set; } = default!;

    [MaxLength(36)]
    public string AppointmentId { get; set; } = default!; // external appointment aggregate id

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

[Table("Lab_Test_Type", Schema = "documents")]
public class LabTestType
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Link to LOINC
    [Required]
    [MaxLength(20)]
    public string LoincCode { get; set; } = default!;

    // Denormalized facets from LOINC for convenience
    [MaxLength(500)] public string? Name { get; set; } // Long common name
    [MaxLength(255)] public string? LoincComponent { get; set; }
    [MaxLength(50)] public string? LoincProperty { get; set; }
    [MaxLength(50)] public string? LoincTime { get; set; }
    [MaxLength(512)] public string? LoincSystem { get; set; }
    [MaxLength(50)] public string? LoincScale { get; set; }
    [MaxLength(100)] public string? LoincMethod { get; set; }
    [MaxLength(100)] public string? ExampleUnits { get; set; }
    [MaxLength(200)] public string? ReferenceRange { get; set; }
}
