using Microsoft.EntityFrameworkCore;
using AppointmentService.Models;
using MassTransit;


namespace AppointmentService.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<AppointmentCategory> AppointmentCategories => Set<AppointmentCategory>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("Appointment", schema: "appointment");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasDefaultValueSql("NEWID()");
            e.Property(a => a.PatientId).IsRequired();
            e.Property(a => a.DoctorId).IsRequired();
            e.Property(a => a.Status).HasMaxLength(50).HasDefaultValue("Scheduled");
            e.Property(a => a.AppointmentType).HasMaxLength(100);
            e.Property(a => a.Notes).HasMaxLength(500);
            e.Property(a => a.RoomId);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(a => a.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(a => a.UpcomingNotificationSentAt).HasColumnType("datetime2");
            e.Property(a => a.ThirtyMinNotificationSentAt).HasColumnType("datetime2");
            e.HasIndex(a => a.PatientId);
            e.HasIndex(a => a.DoctorId);
            e.HasIndex(a => a.ScheduledAt);
            // Composite index to support fast lookup for overdue transitions
            e.HasIndex(a => new { a.Status, a.ScheduledEndAt });
        });

        modelBuilder.Entity<AppointmentSlot>(e =>
        {
            e.ToTable("Appointment_Slot", schema: "appointment");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("NEWID()");
            e.Property(s => s.DoctorId).IsRequired();
            e.Property(s => s.IsAvailable).HasDefaultValue(true);
            e.Property(s => s.AppointmentId);
            e.Property(s => s.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(s => s.DoctorId);
            e.HasIndex(s => s.StartTime);
        });

        modelBuilder.Entity<Schedule>(e =>
        {
            e.ToTable("Schedule", schema: "appointment");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("NEWID()");
            e.Property(s => s.DoctorId).IsRequired();
            e.Property(s => s.IsActive).HasDefaultValue(true);
            e.Property(s => s.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(s => s.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(s => s.DoctorId);
        });

        modelBuilder.Entity<AppointmentCategory>(e =>
        {
            e.ToTable("Appointment_Category", schema: "appointment");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("NEWID()");
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.IsActive).HasDefaultValue(true);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(c => c.Name);
        });
    }
}
