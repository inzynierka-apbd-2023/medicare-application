using Microsoft.EntityFrameworkCore;
using LabService.Models;

namespace LabService.Data;

public class LabDbContext : DbContext
{
    public LabDbContext(DbContextOptions<LabDbContext> options) : base(options) { }

    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabTest> LabTests => Set<LabTest>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<LabResultReview> LabResultReviews => Set<LabResultReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LabOrder>(e =>
        {
            e.ToTable("Lab_Order", schema: "lab");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(o => o.PatientId).HasMaxLength(36).IsRequired();
            e.Property(o => o.OrderingDoctorId).HasMaxLength(36).IsRequired();
            e.Property(o => o.MedicalRecordId).HasMaxLength(36);
            e.Property(o => o.Status).HasMaxLength(50).HasDefaultValue("Ordered");
            e.Property(o => o.ClinicalNotes).HasMaxLength(500);
            e.Property(o => o.Priority).HasMaxLength(50).HasDefaultValue("Normal");
            e.Property(o => o.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(o => o.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(o => o.PatientId);
            e.HasIndex(o => o.OrderingDoctorId);
            e.HasIndex(o => o.OrderedDate);
        });

        modelBuilder.Entity<LabTest>(e =>
        {
            e.ToTable("Lab_Test", schema: "lab");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(t => t.LabOrderId).HasMaxLength(36).IsRequired();
            e.Property(t => t.LoincCode).HasMaxLength(20).IsRequired();
            e.Property(t => t.TestName).HasMaxLength(200).IsRequired();
            e.Property(t => t.Status).HasMaxLength(50).HasDefaultValue("Pending");
            e.Property(t => t.Instructions).HasMaxLength(500);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(t => t.LabOrderId);
            e.HasIndex(t => t.LoincCode);
        });

        modelBuilder.Entity<LabResult>(e =>
        {
            e.ToTable("Lab_Result", schema: "lab");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(r => r.LabTestId).HasMaxLength(36).IsRequired();
            e.Property(r => r.PatientId).HasMaxLength(36).IsRequired();
            e.Property(r => r.Value).HasMaxLength(500);
            e.Property(r => r.Unit).HasMaxLength(100);
            e.Property(r => r.ReferenceRange).HasMaxLength(200);
            e.Property(r => r.Flag).HasMaxLength(50);
            e.Property(r => r.Comments).HasMaxLength(1000);
            e.Property(r => r.ReviewedByDoctorId).HasMaxLength(36);
            e.Property(r => r.ReviewStatus).HasMaxLength(50).HasDefaultValue("Pending");
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(r => r.PatientId);
            e.HasIndex(r => r.LabTestId);
            e.HasIndex(r => r.ResultDate);
        });

        modelBuilder.Entity<LabResultReview>(e =>
        {
            e.ToTable("Lab_Result_Review", schema: "lab");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(r => r.LabResultId).HasMaxLength(36).IsRequired();
            e.Property(r => r.ReviewedByDoctorId).HasMaxLength(36).IsRequired();
            e.Property(r => r.ReviewStatus).HasMaxLength(50).IsRequired();
            e.Property(r => r.ReviewNotes).HasMaxLength(1000);
            e.Property(r => r.Recommendations).HasMaxLength(1000);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(r => r.LabResultId);
            e.HasIndex(r => r.ReviewedByDoctorId);
        });
    }
}
