using Microsoft.EntityFrameworkCore;
using PractitionerService.Models;

namespace PractitionerService.Data;

public class PractitionerDbContext : DbContext
{
    public PractitionerDbContext(DbContextOptions<PractitionerDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<MedicalService> Services => Set<MedicalService>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<SpecializationService> SpecializationServices => Set<SpecializationService>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doctor>(e =>
        {
            e.ToTable("Doctor", schema: "practitioner");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("Id").HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(d => d.UserId).HasMaxLength(36).IsRequired();
            e.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(d => d.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(d => d.UserId).IsUnique();
        });
        modelBuilder.Entity<Receptionist>(e =>
        {
            e.ToTable("Receptionist", schema: "practitioner");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(r => r.UserId).HasMaxLength(36).IsRequired();
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(r => r.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(r => r.UserId).IsUnique();
        });
        modelBuilder.Entity<MedicalService>(e =>
        {
            e.ToTable("Service", schema: "practitioner");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(s => s.Name).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<Specialization>(e =>
        {
            e.ToTable("Specialization", schema: "practitioner");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(s => s.Name).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<SpecializationService>(e =>
        {
            e.ToTable("Specialization_Service", schema: "practitioner");
            e.HasKey(ss => new { ss.SpecializationId, ss.ServiceId });
            e.Property(ss => ss.SpecializationId).HasMaxLength(36);
            e.Property(ss => ss.ServiceId).HasMaxLength(36);
            e.HasOne<Specialization>().WithMany().HasForeignKey(ss => ss.SpecializationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<MedicalService>().WithMany().HasForeignKey(ss => ss.ServiceId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DoctorSpecialization>(e =>
        {
            e.ToTable("Doctor_Specialization", schema: "practitioner");
            e.HasKey(ds => new { ds.DoctorId, ds.SpecializationId });
            e.Property(ds => ds.DoctorId).HasMaxLength(36);
            e.Property(ds => ds.SpecializationId).HasMaxLength(36);
            e.HasOne<Doctor>().WithMany().HasForeignKey(ds => ds.DoctorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Specialization>().WithMany().HasForeignKey(ds => ds.SpecializationId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DoctorSchedule>(e =>
        {
            e.ToTable("Doctor_Schedule", schema: "practitioner");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasMaxLength(36).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            e.Property(s => s.DoctorId).HasMaxLength(36).IsRequired();
            e.Property(s => s.DayOfWeek).IsRequired();
            e.Property(s => s.StartTime).IsRequired();
            e.Property(s => s.EndTime).IsRequired();
            e.Property(s => s.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(s => s.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne<Doctor>().WithMany(d => d.Schedules).HasForeignKey(s => s.DoctorId).OnDelete(DeleteBehavior.Cascade);
        });

        // Projection view: DoctorDirectory joining user profile
        modelBuilder.Entity<DoctorDirectory>().HasNoKey().ToView("DoctorDirectory", schema: "practitioner");
    }
}
