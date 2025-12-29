using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Data;

public class MedicalCatalogDbContext : DbContext
{
    public MedicalCatalogDbContext(DbContextOptions<MedicalCatalogDbContext> options) : base(options) { }

    public DbSet<Icd10> Icd10 => Set<Icd10>();
    public DbSet<LoincEntry> Loinc => Set<LoincEntry>();
    public DbSet<CatalogRelease> Releases => Set<CatalogRelease>();
    public DbSet<LoincMapTo> LoincMapTo => Set<LoincMapTo>();
    public DbSet<LoincAnswerList> LoincAnswerList => Set<LoincAnswerList>();
    public DbSet<LoincAnswerLink> LoincAnswerLink => Set<LoincAnswerLink>();
    public DbSet<LoincPanel> LoincPanel => Set<LoincPanel>();
    public DbSet<LoincPanelItem> LoincPanelItem => Set<LoincPanelItem>();
    public DbSet<LoincConsumerName> LoincConsumerName => Set<LoincConsumerName>();
    public DbSet<AtcEntry> Atc => Set<AtcEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    // Removed Medical_Condition, Lab_Test_Type

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

    // SNOMED removed

        modelBuilder.Entity<LoincEntry>(e =>
        {
            e.ToTable("loinc", schema: "catalog");
            e.HasKey(x => x.LoincNum);
            e.Property(x => x.LoincNum).HasMaxLength(20);
            e.Property(x => x.LongCommonName).HasMaxLength(500);
            e.Property(x => x.ShortName).HasMaxLength(255);
            e.Property(x => x.Component).HasMaxLength(255);
            e.Property(x => x.Property).HasMaxLength(50);
            e.Property(x => x.TimeAspect).HasMaxLength(50);
            e.Property(x => x.System).HasMaxLength(100);
            e.Property(x => x.ScaleType).HasMaxLength(50);
            e.Property(x => x.MethodType).HasMaxLength(100);
            e.Property(x => x.Class).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(50);
            e.Property(x => x.VersionLastChanged).HasMaxLength(50);
            e.Property(x => x.DefinitionDescription).HasMaxLength(2000);
            e.Property(x => x.ExampleUnits).HasMaxLength(100);
            e.Property(x => x.ExternalCopyrightNotice).HasMaxLength(1000);
            e.Property(x => x.PanelType).HasMaxLength(50);
            e.Property(x => x.Equation).HasMaxLength(2000);
            e.HasIndex(x => x.Component);
            e.HasIndex(x => x.LongCommonName);
            e.HasIndex(x => x.ShortName);
            // Full-text indexes are typically managed outside EF; add in migration using raw SQL if needed.
        });

    // CPT/HCPCS removed

        modelBuilder.Entity<CatalogRelease>(e =>
        {
            e.ToTable("release", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.System).HasMaxLength(50).IsRequired();
            e.Property(x => x.Version).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.System, x.Version }).IsUnique();
        });

        modelBuilder.Entity<LoincMapTo>(e =>
        {
            e.ToTable("loinc_map_to", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.FromLoinc).HasMaxLength(20).IsRequired();
            e.Property(x => x.ToLoinc).HasMaxLength(20).IsRequired();
            e.Property(x => x.MapType).HasMaxLength(50);
            e.Property(x => x.Comment).HasMaxLength(500);
            e.HasIndex(x => new { x.FromLoinc, x.ToLoinc }).IsUnique();
        });

        modelBuilder.Entity<LoincAnswerList>(e =>
        {
            e.ToTable("loinc_answer_list", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.AnswerListId).HasMaxLength(50).IsRequired();
            e.Property(x => x.AnswerStringId).HasMaxLength(50);
            e.Property(x => x.DisplayName).HasMaxLength(255);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => x.AnswerListId);
        });

        modelBuilder.Entity<LoincAnswerLink>(e =>
        {
            e.ToTable("loinc_answer_link", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.LoincNum).HasMaxLength(20).IsRequired();
            e.Property(x => x.AnswerListId).HasMaxLength(50).IsRequired();
            e.Property(x => x.LinkType).HasMaxLength(50);
            e.HasIndex(x => new { x.LoincNum, x.AnswerListId }).IsUnique();
        });

        modelBuilder.Entity<LoincPanel>(e =>
        {
            e.ToTable("loinc_panel", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.PanelLoincNum).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.PanelLoincNum).IsUnique();
        });

        modelBuilder.Entity<LoincPanelItem>(e =>
        {
            e.ToTable("loinc_panel_item", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.PanelLoincNum).HasMaxLength(20).IsRequired();
            e.Property(x => x.ItemLoincNum).HasMaxLength(20).IsRequired();
            e.Property(x => x.Ordinal);
            e.Property(x => x.Optionality).HasMaxLength(50);
            e.HasIndex(x => new { x.PanelLoincNum, x.ItemLoincNum }).IsUnique();
        });

        modelBuilder.Entity<LoincConsumerName>(e =>
        {
            e.ToTable("loinc_consumer_name", schema: "catalog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            e.Property(x => x.LoincNum).HasMaxLength(20).IsRequired();
            e.Property(x => x.ConsumerName).HasMaxLength(255).IsRequired();
            e.Property(x => x.Language).HasMaxLength(20);
            e.HasIndex(x => new { x.LoincNum, x.ConsumerName, x.Language }).IsUnique();
        });

        modelBuilder.Entity<AtcEntry>(e =>
        {
            e.ToTable("atc", schema: "catalog");
            e.HasKey(x => x.AtcCode);
            e.Property(x => x.AtcCode).HasMaxLength(10);
            e.Property(x => x.AtcName).HasMaxLength(500).IsRequired();
            e.Property(x => x.Ddd).HasColumnType("decimal(18,4)");
            e.Property(x => x.Uom).HasMaxLength(50);
            e.Property(x => x.AdmR).HasMaxLength(50);
            e.Property(x => x.Note).HasMaxLength(1000);
            e.HasIndex(x => x.AtcName);
        });
    }
}
