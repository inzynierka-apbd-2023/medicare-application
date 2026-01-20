using Microsoft.EntityFrameworkCore;
using MessagingService.Models;
using MassTransit;

namespace MessagingService.Data;

public class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options) { }

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<ThreadParticipant> ThreadParticipants => Set<ThreadParticipant>();
    public DbSet<ThreadMessage> ThreadMessages => Set<ThreadMessage>();
    public DbSet<MessageReceipt> MessageReceipts => Set<MessageReceipt>();
    public DbSet<PatientDoctorContact> PatientDoctorContacts => Set<PatientDoctorContact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("messaging");

        modelBuilder.AddInboxStateEntity(x => x.ToTable("InboxState", "messaging"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("OutboxMessage", "messaging"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("OutboxState", "messaging"));

        modelBuilder.Entity<Message>(e =>
        {
            e.ToTable("Message", schema: "messaging");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("NEWID()");
            e.Property(m => m.SenderId).IsRequired();
            e.Property(m => m.RecipientId).IsRequired();
            e.Property(m => m.Subject).HasMaxLength(200).IsRequired();
            e.Property(m => m.Content).HasMaxLength(2000).IsRequired();
            e.Property(m => m.MessageType).HasMaxLength(50).HasDefaultValue("General");
            e.Property(m => m.Priority).HasMaxLength(50).HasDefaultValue("Normal");
            e.Property(m => m.IsRead).HasDefaultValue(false);
            e.Property(m => m.RelatedEntityId);
            e.Property(m => m.RelatedEntityType).HasMaxLength(50);
            e.Property(m => m.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(m => m.SenderId);
            e.HasIndex(m => m.RecipientId);
            e.HasIndex(m => m.SentAt);
            e.HasIndex(m => m.IsRead);
        });

        modelBuilder.Entity<MessageThread>(e =>
        {
            e.ToTable("Message_Thread", schema: "messaging");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("NEWID()");
            e.Property(t => t.Subject).HasMaxLength(200).IsRequired();
            e.Property(t => t.InitiatorId).IsRequired();
            e.Property(t => t.IsActive).HasDefaultValue(true);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(t => t.InitiatorId);
        });

        modelBuilder.Entity<ThreadParticipant>(e =>
        {
            e.ToTable("Thread_Participant", schema: "messaging");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("NEWID()");
            e.Property(p => p.ThreadId).IsRequired();
            e.Property(p => p.UserId).IsRequired();
            e.Property(p => p.IsActive).HasDefaultValue(true);
            e.Property(p => p.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(p => p.ThreadId);
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => new { p.ThreadId, p.UserId }).IsUnique();
        });

        modelBuilder.Entity<ThreadMessage>(e =>
        {
            e.ToTable("Thread_Message", schema: "messaging");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("NEWID()");
            e.Property(m => m.ThreadId).IsRequired();
            e.Property(m => m.SenderId).IsRequired();
            e.Property(m => m.Content).HasMaxLength(2000).IsRequired();
            e.Property(m => m.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(m => m.ThreadId);
            e.HasIndex(m => m.SenderId);
            e.HasIndex(m => m.SentAt);
        });

        modelBuilder.Entity<MessageReceipt>(e =>
        {
            e.ToTable("Message_Receipt", schema: "messaging");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasDefaultValueSql("NEWID()");
            e.Property(r => r.MessageId).IsRequired();
            e.Property(r => r.UserId).IsRequired();
            e.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(r => r.MessageId);
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => new { r.MessageId, r.UserId }).IsUnique();
        });

        modelBuilder.Entity<PatientDoctorContact>(e =>
        {
            e.ToTable("Patient_Doctor_Contact", schema: "messaging");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("NEWID()");
            e.Property(c => c.PatientUserId).IsRequired();
            e.Property(c => c.DoctorUserId).IsRequired();
            e.Property(c => c.DoctorName).HasMaxLength(200);
            e.Property(c => c.DoctorSpecialization).HasMaxLength(200);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(c => c.PatientUserId);
            e.HasIndex(c => c.DoctorUserId);
            e.HasIndex(c => new { c.PatientUserId, c.DoctorUserId }).IsUnique();
        });
    }
}
