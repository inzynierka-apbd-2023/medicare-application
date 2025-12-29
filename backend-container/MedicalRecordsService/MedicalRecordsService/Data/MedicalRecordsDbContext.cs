using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Data;

public class MedicalRecordsDbContext : DbContext
{
    public MedicalRecordsDbContext(DbContextOptions<MedicalRecordsDbContext> options) : base(options) { }

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<VitalSigns> VitalSigns => Set<VitalSigns>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.ToTable("Medical_Record", schema: "medical");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("NEWID()");
            e.Property(m => m.PatientId).IsRequired();
            e.Property(m => m.DoctorId).IsRequired();
            e.Property(m => m.AppointmentId);
            e.Property(m => m.ChiefComplaint).HasMaxLength(200);
            e.Property(m => m.HistoryOfPresentIllness).HasMaxLength(1000);
            e.Property(m => m.PhysicalExamination).HasMaxLength(1000);
            e.Property(m => m.Assessment).HasMaxLength(1000);
            e.Property(m => m.Plan).HasMaxLength(1000);
            e.Property(m => m.Notes).HasMaxLength(500);
            e.Property(m => m.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(m => m.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(m => m.PatientId);
            e.HasIndex(m => m.DoctorId);
            e.HasIndex(m => m.VisitDate);
        });

        modelBuilder.Entity<Prescription>(e =>
        {
            e.ToTable("Prescription", schema: "medical");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("NEWID()");
            e.Property(p => p.MedicalRecordId).IsRequired();
            e.Property(p => p.PatientId).IsRequired();
            e.Property(p => p.DoctorId).IsRequired();
            e.Property(p => p.MedicationName).HasMaxLength(200).IsRequired();
            e.Property(p => p.AtcCode).HasMaxLength(10);
            e.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
            e.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
            e.Property(p => p.Instructions).HasMaxLength(500);
            e.Property(p => p.Status).HasMaxLength(50).HasDefaultValue("Active");
            e.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(p => p.PatientId);
            e.HasIndex(p => p.MedicalRecordId);
        });

        modelBuilder.Entity<Diagnosis>(e =>
        {
            e.ToTable("Diagnosis", schema: "medical");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasDefaultValueSql("NEWID()");
            e.Property(d => d.MedicalRecordId).IsRequired();
            e.Property(d => d.Icd10Code).HasMaxLength(10).IsRequired();
            e.Property(d => d.Description).HasMaxLength(500).IsRequired();
            e.Property(d => d.Type).HasMaxLength(50).HasDefaultValue("Primary");
            e.Property(d => d.Notes).HasMaxLength(500);
            e.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(d => d.MedicalRecordId);
            e.HasIndex(d => d.Icd10Code);
        });

        modelBuilder.Entity<VitalSigns>(e =>
        {
            e.ToTable("Vital_Signs", schema: "medical");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasDefaultValueSql("NEWID()");
            e.Property(v => v.MedicalRecordId).IsRequired();
            e.Property(v => v.PatientId).IsRequired();
            e.Property(v => v.Temperature).HasColumnType("decimal(4,1)");
            e.Property(v => v.OxygenSaturation).HasColumnType("decimal(5,2)");
            e.Property(v => v.Height).HasColumnType("decimal(5,2)");
            e.Property(v => v.Weight).HasColumnType("decimal(5,2)");
            e.Property(v => v.Notes).HasMaxLength(500);
            e.Property(v => v.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(v => v.PatientId);
            e.HasIndex(v => v.MedicalRecordId);
            e.HasIndex(v => v.MeasuredAt);
        });
    }
}
