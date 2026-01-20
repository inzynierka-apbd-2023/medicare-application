using Microsoft.EntityFrameworkCore;
using ArchiveService.Models;
using MassTransit;

namespace ArchiveService.Data;

public class ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : DbContext(options)
{
    public DbSet<ArchivedDoctor> ArchivedDoctors => Set<ArchivedDoctor>();
    public DbSet<ArchivedDocument> ArchivedDocuments => Set<ArchivedDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<ArchivedDoctor>(b =>
        {
            b.ToTable("ArchivedDoctor", schema: "archive");
            b.HasKey(x => x.DoctorId);
            b.Property(x => x.FullName).HasMaxLength(256);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(64);
            b.Property(x => x.SpecializationIdsJson).HasColumnName("SpecializationIdsJson");
            b.Property(x => x.SnapshotJson);
        });

        modelBuilder.Entity<ArchivedDocument>(b =>
        {
            b.ToTable("ArchivedDocument", schema: "archive");
            b.HasKey(x => x.DocumentId);
            b.Property(x => x.Type).HasMaxLength(64);
            b.Property(x => x.Title).HasMaxLength(512);
            b.Property(x => x.SnapshotJson);
            b.HasIndex(x => x.DoctorId);
        });
    }
}
