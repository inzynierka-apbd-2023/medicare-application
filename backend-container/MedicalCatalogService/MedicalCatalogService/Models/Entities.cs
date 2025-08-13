using System.ComponentModel.DataAnnotations;

namespace MedicalCatalogService.Models;

public class MedicalCondition
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(50)]
    public string Code { get; set; } = default!; // e.g., ICD-10 code
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}

// Catalog terminology entities (owned by schema: catalog)
public class Icd10
{
    [Key]
    [MaxLength(10)]
    public string Code { get; set; } = default!; // e.g., E11.9
    [Required, MaxLength(500)]
    public string Title { get; set; } = default!;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    [MaxLength(50)]
    public string? Status { get; set; } // Active/Inactive/Deprecated
}

public class SnomedConcept
{
    [Key]
    public long ConceptId { get; set; }
    [MaxLength(500)]
    public string? Fsn { get; set; } // Fully specified name
    [MaxLength(255)]
    public string? PreferredTerm { get; set; }
    public bool Active { get; set; }
    public DateTime? EffectiveTime { get; set; }
}

public class LoincEntry
{
    [Key]
    [MaxLength(20)]
    public string LoincNum { get; set; } = default!; // e.g., 718-7
    [MaxLength(255)] public string? Component { get; set; }
    [MaxLength(50)] public string? Property { get; set; }
    [MaxLength(50)] public string? TimeAspct { get; set; }
    [MaxLength(100)] public string? System { get; set; }
    [MaxLength(50)] public string? ScaleTyp { get; set; }
    [MaxLength(100)] public string? MethodTyp { get; set; }
    [MaxLength(500)] public string? LongCommonName { get; set; }
}

public class CptCode
{
    [Key]
    [MaxLength(10)]
    public string Code { get; set; } = default!;
    [MaxLength(255)] public string? ShortDesc { get; set; }
    [MaxLength(1000)] public string? LongDesc { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class HcpcsCode
{
    [Key]
    [MaxLength(10)]
    public string Code { get; set; } = default!;
    [MaxLength(255)] public string? ShortDesc { get; set; }
    [MaxLength(1000)] public string? LongDesc { get; set; }
    [MaxLength(50)] public string? ModifierFlags { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class CatalogRelease
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string System { get; set; } = default!; // icd10, snomed, loinc, cpt, hcpcs
    [Required, MaxLength(100)]
    public string Version { get; set; } = default!; // e.g., 2025-10, 2025-02
    public DateTime ReleasedOn { get; set; }
}

// Optional crosswalk mapping table
public class CodeMapping
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)] public string SourceSystem { get; set; } = default!;
    [Required, MaxLength(100)] public string SourceCode { get; set; } = default!;
    [Required, MaxLength(50)] public string TargetSystem { get; set; } = default!;
    [Required, MaxLength(100)] public string TargetCode { get; set; } = default!;
    public double? Confidence { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public class LabTestType
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = default!;
    [Required, MaxLength(50)]
    public string Code { get; set; } = default!; // e.g., LOINC
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;
    [MaxLength(50)]
    public string? Unit { get; set; } // e.g., mg/dL
    [MaxLength(200)]
    public string? ReferenceRange { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
