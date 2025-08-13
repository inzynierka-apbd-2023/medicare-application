using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MedicalCatalogService.Data;

#nullable disable

namespace MedicalCatalogService.Migrations
{
    [DbContext(typeof(MedicalCatalogDbContext))]
    partial class MedicalCatalogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("MedicalCatalogService.Models.MedicalCondition", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("nvarchar(36)")
                    .HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");

                b.Property<string>("Code")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Description")
                    .HasMaxLength(1000)
                    .HasColumnType("nvarchar(1000)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                b.HasKey("Id");

                b.HasIndex("Code")
                    .IsUnique();

                b.ToTable("Medical_Condition", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LabTestType", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(36)
                    .HasColumnType("nvarchar(36)")
                    .HasDefaultValueSql("CONVERT(VARCHAR(36), NEWID())");

                b.Property<string>("Code")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("ReferenceRange")
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                b.Property<string>("Unit")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                b.HasKey("Id");

                b.HasIndex("Code")
                    .IsUnique();

                b.ToTable("Lab_Test_Type", "catalog");
            });
#pragma warning restore 612, 618
        }
    }
}
