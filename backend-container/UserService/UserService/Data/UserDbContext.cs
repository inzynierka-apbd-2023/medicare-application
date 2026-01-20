using Microsoft.EntityFrameworkCore;
using UserService.Models;
using MassTransit;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity(x => x.ToTable("InboxState", "user"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("OutboxMessage", "user"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("OutboxState", "user"));

        const string SysUtc = "SYSUTCDATETIME()";

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
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


        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("Refresh_Token", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SysUtc);
            entity.Property(e => e.ExpiresAt).HasDefaultValueSql("DATEADD(day,7,SYSUTCDATETIME())");
            entity.Property(e => e.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt });
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("Password_Reset_Token", schema: "user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SysUtc);
            entity.Property(e => e.TokenHash).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });
    }
}
