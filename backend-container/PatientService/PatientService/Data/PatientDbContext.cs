using Microsoft.EntityFrameworkCore;
using PatientService.Models;
using MassTransit;

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
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity(x => x.ToTable("InboxState", "patient"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("OutboxMessage", "patient"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("OutboxState", "patient"));

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patient", schema: "patient");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("NEWID()");
            e.Property(p => p.UserId).IsRequired();
            e.Property(p => p.PrimaryDoctorId);
            e.Property(p => p.BloodType).HasMaxLength(10);
            e.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(p => p.UserId).IsUnique();
        });
        modelBuilder.Entity<EmergencyContact>(e =>
        {
            e.ToTable("Emergency_Contact", schema: "patient");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("NEWID()");
            e.Property(c => c.PatientId).IsRequired();
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Relation).HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(100);
        });
        modelBuilder.Entity<Insurance>(e =>
        {
            e.ToTable("Insurance", schema: "patient");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasDefaultValueSql("NEWID()");
            e.Property(i => i.PatientId).IsRequired();
            e.Property(i => i.Provider).HasMaxLength(200);
            e.Property(i => i.PolicyNumber).HasMaxLength(100);
        });
        modelBuilder.Entity<PatientStatus>(e =>
        {
            e.ToTable("Patient_Status", schema: "patient");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("NEWID()");
            e.Property(s => s.PatientId).IsRequired();
            e.Property(s => s.Status).HasMaxLength(50).IsRequired();
            e.Property(s => s.EffectiveAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(s => s.IdempotencyKey).HasMaxLength(100);
            e.HasIndex(s => new { s.PatientId, s.EffectiveAt });
            e.HasIndex(s => s.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
        });

        // View mapping - view is created at startup in Program.cs
        modelBuilder.Entity<PatientOverview>().HasNoKey().ToView("PatientOverview", schema: "patient");
    }
}
