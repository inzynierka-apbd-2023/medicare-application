using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Data;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<Insurance> Insurances => Set<Insurance>();
    public DbSet<PatientStatus> PatientStatuses => Set<PatientStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patient", schema: "patient");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(p => p.UserId).HasMaxLength(36).IsRequired();
            e.Property(p => p.PrimaryDoctorId).HasMaxLength(36);
            e.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(p => p.UserId).IsUnique();
        });
        modelBuilder.Entity<EmergencyContact>(e =>
        {
            e.ToTable("Emergency_Contact", schema: "patient");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(c => c.PatientId).HasMaxLength(36).IsRequired();
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Relation).HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(100);
        });
        modelBuilder.Entity<Insurance>(e =>
        {
            e.ToTable("Insurance", schema: "patient");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(i => i.PatientId).HasMaxLength(36).IsRequired();
            e.Property(i => i.Provider).HasMaxLength(200);
            e.Property(i => i.PolicyNumber).HasMaxLength(100);
        });
        modelBuilder.Entity<PatientStatus>(e =>
        {
            e.ToTable("Patient_Status", schema: "patient");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(s => s.PatientId).HasMaxLength(36).IsRequired();
            e.Property(s => s.Status).HasMaxLength(50).IsRequired();
            e.Property(s => s.EffectiveAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(s => new { s.PatientId, s.EffectiveAt });
        });

        modelBuilder.Entity<PatientOverview>().HasNoKey().ToView("PatientOverview", schema: "patient");
    }
}
