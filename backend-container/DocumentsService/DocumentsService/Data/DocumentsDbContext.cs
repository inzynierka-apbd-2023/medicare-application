using Microsoft.EntityFrameworkCore;
using DocumentsService.Models;
using MassTransit;

namespace DocumentsService.Data;

public class DocumentsDbContext : DbContext
{
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<VisitDocument> VisitDocuments => Set<VisitDocument>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<SickLeave> SickLeaves => Set<SickLeave>();
    public DbSet<LabResults> LabResults => Set<LabResults>();
    public DbSet<LabTestResult> LabTestResults => Set<LabTestResult>();
    public DbSet<LabTestType> LabTestTypes => Set<LabTestType>();
    public DbSet<DocumentAssignment> DocumentAssignments => Set<DocumentAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity(x => x.ToTable("InboxState", "documents"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("OutboxMessage", "documents"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("OutboxState", "documents"));

    const string SysUtc = "SYSUTCDATETIME()";
    const string SqlGuid = "NEWID()";

        modelBuilder.HasDefaultSchema("documents");

        modelBuilder.Entity<Document>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.CreatedAt).HasDefaultValueSql(SysUtc);
            e.HasIndex(p => new { p.PatientId, p.Type, p.CreatedAt });
            e.HasIndex(p => new { p.DoctorId, p.CreatedAt });
            e.HasOne(p => p.DocumentType).WithMany().HasForeignKey(p => p.DocumentTypeId);
            e.HasOne(p => p.VisitDocument).WithOne().HasForeignKey<VisitDocument>(v => v.DocumentId);
            e.HasOne(p => p.Prescription).WithOne().HasForeignKey<Prescription>(v => v.DocumentId);
            e.HasOne(p => p.Referral).WithOne().HasForeignKey<Referral>(v => v.DocumentId);
            e.HasOne(p => p.SickLeave).WithOne().HasForeignKey<SickLeave>(v => v.DocumentId);
            e.HasOne(p => p.LabResults).WithOne().HasForeignKey<LabResults>(v => v.DocumentId);
        });

        modelBuilder.Entity<DocumentType>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasIndex(p => p.Code).IsUnique();
        });

        modelBuilder.Entity<VisitDocument>(e =>
        {
            e.HasKey(p => p.DocumentId);
        });
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(p => p.DocumentId);
            e.Property(p => p.AtcCode).HasMaxLength(20);
            e.Property(p => p.AtcName).HasMaxLength(500);
            e.HasIndex(p => p.AtcCode);
        });
        modelBuilder.Entity<Referral>(e =>
        {
            e.HasKey(p => p.DocumentId);
        });
        modelBuilder.Entity<SickLeave>(e =>
        {
            e.HasKey(p => p.DocumentId);
        });
        modelBuilder.Entity<LabResults>(e =>
        {
            e.HasKey(p => p.DocumentId);
        });
        modelBuilder.Entity<LabTestResult>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.NumericValue).HasPrecision(18, 2);
            e.HasIndex(p => p.LabResultsDocumentId);
            e.Property(p => p.LoincCode).HasMaxLength(20);
            e.HasOne(p => p.LabTestType)
                .WithMany()
                .HasForeignKey(p => p.LabTestTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<DocumentAssignment>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasIndex(p => new { p.DocumentId, p.AppointmentId }).IsUnique();
            e.Property(p => p.AssignedAt).HasDefaultValueSql(SysUtc);
        });

        modelBuilder.Entity<LabTestType>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.LoincCode).IsRequired().HasMaxLength(20);
            e.HasIndex(p => p.LoincCode).IsUnique();
            e.ToTable("Lab_Test_Type", "documents");
        });
    }
}
