using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Data;

public class MedicalCatalogDbContext : DbContext
{
    public MedicalCatalogDbContext(DbContextOptions<MedicalCatalogDbContext> options) : base(options) { }

    public DbSet<MedicalCondition> MedicalConditions => Set<MedicalCondition>();
    public DbSet<LabTestType> LabTestTypes => Set<LabTestType>();
    public DbSet<Icd10> Icd10 => Set<Icd10>();
    public DbSet<SnomedConcept> Snomed => Set<SnomedConcept>();
    public DbSet<LoincEntry> Loinc => Set<LoincEntry>();
    public DbSet<CptCode> Cpt => Set<CptCode>();
    public DbSet<HcpcsCode> Hcpcs => Set<HcpcsCode>();
    public DbSet<CatalogRelease> Releases => Set<CatalogRelease>();
    public DbSet<CodeMapping> Mappings => Set<CodeMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalCondition>(e =>
        {
            e.ToTable("Medical_Condition", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<LabTestType>(e =>
        {
            e.ToTable("Lab_Test_Type", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(50);
            e.Property(x => x.ReferenceRange).HasMaxLength(200);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.Code).IsUnique();
        });

        // Catalog schema tables
        modelBuilder.Entity<Icd10>(e =>
        {
            e.ToTable("icd10", schema: "catalog");
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(10);
            e.Property(x => x.Title).HasMaxLength(500).IsRequired();
            e.Property(x => x.Status).HasMaxLength(50);
            e.HasIndex(x => x.Title);
        });

        modelBuilder.Entity<SnomedConcept>(e =>
        {
            e.ToTable("snomed", schema: "catalog");
            e.HasKey(x => x.ConceptId);
            e.Property(x => x.Fsn).HasMaxLength(500);
            e.Property(x => x.PreferredTerm).HasMaxLength(255);
            e.HasIndex(x => x.Active);
            e.HasIndex(x => x.PreferredTerm);
        });

        modelBuilder.Entity<LoincEntry>(e =>
        {
            e.ToTable("loinc", schema: "catalog");
            e.HasKey(x => x.LoincNum);
            e.Property(x => x.LoincNum).HasMaxLength(20);
            e.Property(x => x.Component).HasMaxLength(255);
            e.Property(x => x.Property).HasMaxLength(50);
            e.Property(x => x.TimeAspct).HasMaxLength(50);
            e.Property(x => x.System).HasMaxLength(100);
            e.Property(x => x.ScaleTyp).HasMaxLength(50);
            e.Property(x => x.MethodTyp).HasMaxLength(100);
            e.Property(x => x.LongCommonName).HasMaxLength(500);
            e.HasIndex(x => x.Component);
            e.HasIndex(x => x.LongCommonName);
        });

        modelBuilder.Entity<CptCode>(e =>
        {
            e.ToTable("cpt", schema: "catalog");
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(10);
            e.Property(x => x.ShortDesc).HasMaxLength(255);
            e.Property(x => x.LongDesc).HasMaxLength(1000);
        });

        modelBuilder.Entity<HcpcsCode>(e =>
        {
            e.ToTable("hcpcs", schema: "catalog");
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(10);
            e.Property(x => x.ShortDesc).HasMaxLength(255);
            e.Property(x => x.LongDesc).HasMaxLength(1000);
            e.Property(x => x.ModifierFlags).HasMaxLength(50);
        });

        modelBuilder.Entity<CatalogRelease>(e =>
        {
            e.ToTable("release", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.System).HasMaxLength(50).IsRequired();
            e.Property(x => x.Version).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.System, x.Version }).IsUnique();
        });

        modelBuilder.Entity<CodeMapping>(e =>
        {
            e.ToTable("mappings", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.SourceSystem).HasMaxLength(50).IsRequired();
            e.Property(x => x.SourceCode).HasMaxLength(100).IsRequired();
            e.Property(x => x.TargetSystem).HasMaxLength(50).IsRequired();
            e.Property(x => x.TargetCode).HasMaxLength(100).IsRequired();
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => new { x.SourceSystem, x.SourceCode });
            e.HasIndex(x => new { x.TargetSystem, x.TargetCode });
        });
    }
}
