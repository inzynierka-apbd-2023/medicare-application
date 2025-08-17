using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    const string SysUtc = "SYSUTCDATETIME()";

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SysUtc);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(SysUtc);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId);
                
            entity.HasOne(e => e.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("User_Profile", schema: "user");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SysUtc);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(SysUtc);
            
            entity.HasIndex(e => e.Email).IsUnique();
        });
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.ToTable("Outbox_Event", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
            entity.Property(e => e.OccurredAt).HasDefaultValueSql(SysUtc);
            entity.HasIndex(e => e.PublishedAt);
        });
    }
}
