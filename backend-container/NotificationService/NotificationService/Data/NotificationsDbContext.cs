using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

using MassTransit;

namespace NotificationService.Data;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");

        modelBuilder.AddInboxStateEntity(x => x.ToTable("InboxState", "notifications"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("OutboxMessage", "notifications"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("OutboxState", "notifications"));


        var e = modelBuilder.Entity<Notification>();
        e.ToTable("Notification");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        e.Property(x => x.Recipient_User_Id).IsRequired();
        e.Property(x => x.Description).HasColumnType("nvarchar(255)");
        e.Property(x => x.Type).HasColumnType("tinyint");
        e.Property(x => x.Creation_Date).HasColumnType("datetime");
        e.Property(x => x.Source_Service).HasColumnType("nvarchar(64)");
        e.Property(x => x.Is_Read).HasColumnType("bit");
        e.Property(x => x.Action_Url).HasColumnType("nvarchar(500)");
        e.Property(x => x.Priority_Level).HasColumnType("nvarchar(20)");
        e.Property(x => x.Expires_At).HasColumnType("datetime");

        base.OnModelCreating(modelBuilder);
    }
}
