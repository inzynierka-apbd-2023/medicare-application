using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ArchiveService.Data;

namespace ArchiveService.Migrations;

[DbContext(typeof(ArchiveDbContext))]
partial class ArchiveDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

        modelBuilder.Entity("ArchiveService.Models.ArchivedDoctor", b =>
        {
            b.Property<Guid>("DoctorId").HasColumnType("TEXT");
            b.Property<DateTime>("ArchivedAtUtc").HasColumnType("TEXT");
            b.Property<string>("Email").HasMaxLength(256).HasColumnType("TEXT");
            b.Property<string>("FullName").IsRequired().HasMaxLength(256).HasColumnType("TEXT");
            b.Property<string>("Phone").HasMaxLength(64).HasColumnType("TEXT");
            b.Property<string>("SnapshotJson").HasColumnType("TEXT");
            b.Property<string>("SpecializationIdsJson").HasColumnType("TEXT");
            b.Property<Guid?>("UserId").HasColumnType("TEXT");
            b.HasKey("DoctorId");
            b.ToTable("ArchivedDoctors");
        });

        modelBuilder.Entity("ArchiveService.Models.ArchivedDocument", b =>
        {
            b.Property<Guid>("DocumentId").HasColumnType("TEXT");
            b.Property<DateTime>("ArchivedAtUtc").HasColumnType("TEXT");
            b.Property<Guid>("DoctorId").HasColumnType("TEXT");
            b.Property<Guid?>("PatientId").HasColumnType("TEXT");
            b.Property<string>("SnapshotJson").HasColumnType("TEXT");
            b.Property<string>("Title").HasMaxLength(512).HasColumnType("TEXT");
            b.Property<string>("Type").IsRequired().HasMaxLength(64).HasColumnType("TEXT");
            b.HasKey("DocumentId");
            b.HasIndex("DoctorId");
            b.ToTable("ArchivedDocuments");
        });
    }
}
