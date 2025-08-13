using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PractitionerService.Data;

#nullable disable

namespace PractitionerService.Migrations
{
    [DbContext(typeof(PractitionerDbContext))]
    partial class PractitionerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("PractitionerService.Models.Doctor", b =>
            {
                b.Property<string>("Id").HasMaxLength(36).HasColumnType("nvarchar(36)").HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
                b.Property<string>("Bio").HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<string>("UserId").HasMaxLength(36).HasColumnType("nvarchar(36)");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Doctor", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.Receptionist", b =>
            {
                b.Property<string>("Id").HasMaxLength(36).HasColumnType("nvarchar(36)").HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<string>("UserId").HasMaxLength(36).HasColumnType("nvarchar(36)");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Receptionist", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.MedicalService", b =>
            {
                b.Property<string>("Id").HasMaxLength(36).HasColumnType("nvarchar(36)").HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
                b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<string>("Name").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.HasKey("Id");
                b.ToTable("Service", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.Specialization", b =>
            {
                b.Property<string>("Id").HasMaxLength(36).HasColumnType("nvarchar(36)").HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
                b.Property<string>("Name").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.HasKey("Id");
                b.ToTable("Specialization", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.DoctorSpecialization", b =>
            {
                b.Property<string>("DoctorId").HasMaxLength(36).HasColumnType("nvarchar(36)");
                b.Property<string>("SpecializationId").HasMaxLength(36).HasColumnType("nvarchar(36)");
                b.HasKey("DoctorId", "SpecializationId");
                b.ToTable("Doctor_Specialization", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.DoctorSchedule", b =>
            {
                b.Property<string>("Id").HasMaxLength(36).HasColumnType("nvarchar(36)").HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");
                b.Property<int>("DayOfWeek").HasColumnType("int");
                b.Property<string>("DoctorId").HasMaxLength(36).HasColumnType("nvarchar(36)");
                b.Property<TimeSpan>("EndTime").HasColumnType("time");
                b.Property<TimeSpan>("StartTime").HasColumnType("time");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.HasKey("Id");
                b.ToTable("Doctor_Schedule", "practitioner");
            });

            modelBuilder.Entity("PractitionerService.Models.DoctorDirectory", b =>
            {
                b.HasNoKey();
                b.ToView("DoctorDirectory", "practitioner");
            });
#pragma warning restore 612, 618
        }
    }
}
