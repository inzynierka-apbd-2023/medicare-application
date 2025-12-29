using Microsoft.EntityFrameworkCore;
using AppointmentService.Models;

namespace AppointmentService.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<AppointmentCategory> AppointmentCategories => Set<AppointmentCategory>();
    
    // Analytics entities - read-only views of the main database
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<ScheduleAppointment> ScheduleAppointments => Set<ScheduleAppointment>();
    public DbSet<ScheduleAppointmentStatus> ScheduleAppointmentStatuses => Set<ScheduleAppointmentStatus>();
    public DbSet<AppointmentPayment> AppointmentPayments => Set<AppointmentPayment>();
    public DbSet<Rate> Rates => Set<Rate>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        // Configure analytics entities (read-only, no schema prefix as they reference main DB)
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id);
            e.Property(u => u.Role_Id);
            e.Property(u => u.Schedule_Id);
            // Do not let EF migrations manage this table (exists in main DB)
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(up => up.User_Id);
            e.Property(up => up.User_Id);
            e.Property(up => up.FirstName).HasMaxLength(100);
            e.Property(up => up.LastName).HasMaxLength(100);
            e.Property(up => up.Email).HasMaxLength(255);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<Specialization>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(200);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<DoctorSpecialization>(e =>
        {
            e.HasKey(ds => ds.Id);
            e.Property(ds => ds.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<ScheduleAppointment>(e =>
        {
            e.HasKey(sa => sa.Id);
            e.Property(sa => sa.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<ScheduleAppointmentStatus>(e =>
        {
            e.HasKey(sas => sas.Id);
            e.Property(sas => sas.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<AppointmentPayment>(e =>
        {
            e.HasKey(ap => ap.Id);
            e.Property(ap => ap.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<Rate>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Id);
            e.ToTable(tb => tb.ExcludeFromMigrations());
        });
    }
}
