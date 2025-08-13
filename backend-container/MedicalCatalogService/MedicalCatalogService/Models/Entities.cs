using System.ComponentModel.DataAnnotations;

namespace MedicalCatalogService.Models;

// Removed legacy: MedicalCondition, LabTestType, SNOMED, CPT, HCPCS

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

// SNOMED removed

public class LoincEntry
{
    [Key]
    [MaxLength(20)]
    public string LoincNum { get; set; } = default!; // e.g., 718-7
    [MaxLength(500)] public string? LongCommonName { get; set; }
    [MaxLength(255)] public string? ShortName { get; set; }
    [MaxLength(255)] public string? Component { get; set; }
    [MaxLength(50)] public string? Property { get; set; }
    [MaxLength(50)] public string? TimeAspect { get; set; }
    [MaxLength(512)] public string? System { get; set; }
    [MaxLength(50)] public string? ScaleType { get; set; }
    [MaxLength(100)] public string? MethodType { get; set; }
    [MaxLength(100)] public string? Class { get; set; }
    [MaxLength(50)] public string? Status { get; set; }
    [MaxLength(50)] public string? VersionLastChanged { get; set; }
    // NVARCHAR(MAX) in DB
    public string? DefinitionDescription { get; set; }
    [MaxLength(100)] public string? ExampleUnits { get; set; }
    // NVARCHAR(MAX) in DB
    public string? ExternalCopyrightNotice { get; set; }
    [MaxLength(50)] public string? PanelType { get; set; }
    // NVARCHAR(MAX) in DB
    public string? Equation { get; set; }
}

// CPT removed

// HCPCS removed

public class CatalogRelease
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string System { get; set; } = default!; // icd10, snomed, loinc, cpt, hcpcs
    [Required, MaxLength(100)]
    public string Version { get; set; } = default!; // e.g., 2025-10, 2025-02
    public DateTime ReleasedOn { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
}

// Optional crosswalk mapping table
// Crosswalk mapping deprecated in favor of dedicated LOINC tables

public class LoincMapTo
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(20)] public string FromLoinc { get; set; } = default!;
    [Required, MaxLength(20)] public string ToLoinc { get; set; } = default!;
    [MaxLength(50)] public string? MapType { get; set; }
    [MaxLength(500)] public string? Comment { get; set; }
}

public class LoincAnswerList
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)] public string AnswerListId { get; set; } = default!; // e.g., LL6136-7
    [MaxLength(50)] public string? AnswerStringId { get; set; }
    [MaxLength(255)] public string? DisplayName { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
}

public class LoincAnswerLink
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(20)] public string LoincNum { get; set; } = default!;
    [Required, MaxLength(50)] public string AnswerListId { get; set; } = default!;
    [MaxLength(50)] public string? LinkType { get; set; }
}

public class LoincPanel
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(20)] public string PanelLoincNum { get; set; } = default!;
}

public class LoincPanelItem
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(20)] public string PanelLoincNum { get; set; } = default!;
    [Required, MaxLength(20)] public string ItemLoincNum { get; set; } = default!;
    public int? Ordinal { get; set; }
    [MaxLength(50)] public string? Optionality { get; set; }
}

// LabTestType removed

public class LoincConsumerName
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(20)] public string LoincNum { get; set; } = default!;
    [Required, MaxLength(255)] public string ConsumerName { get; set; } = default!;
    [MaxLength(20)] public string? Language { get; set; }
}
