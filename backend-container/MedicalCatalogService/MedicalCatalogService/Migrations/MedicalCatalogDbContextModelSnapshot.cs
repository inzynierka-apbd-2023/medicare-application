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

            modelBuilder.Entity("MedicalCatalogService.Models.Icd10", b =>
            {
                b.Property<string>("Code")
                    .HasMaxLength(10)
                    .HasColumnType("nvarchar(10)");

                b.Property<DateTime?>("EffectiveFrom")
                    .HasColumnType("datetime2");

                b.Property<DateTime?>("EffectiveTo")
                    .HasColumnType("datetime2");

                b.Property<string>("Status")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.HasKey("Code");

                b.HasIndex("Title");

                b.ToTable("icd10", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincEntry", b =>
            {
                b.Property<string>("LoincNum")
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("Component")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("LongCommonName")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("MethodTyp")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Property")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("ScaleTyp")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("System")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("TimeAspct")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.HasKey("LoincNum");

                b.HasIndex("Component");

                b.HasIndex("LongCommonName");

                b.ToTable("loinc", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.CatalogRelease", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<DateTime>("ReleasedOn")
                    .HasColumnType("datetime2");

                b.Property<string>("System")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Version")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("System", "Version")
                    .IsUnique();

                b.ToTable("release", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincMapTo", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Comment")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("MapToScaleTyp")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("SourceLoinc")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("Status")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("TargetLoinc")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("SourceLoinc", "TargetLoinc")
                    .IsUnique();

                b.ToTable("loinc_map_to", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincAnswerList", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("AnswerId")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("AnswerListId")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("AnswerString")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("DisplayText")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("ExtCode")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("ExtCodeSystem")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.HasKey("Id");

                b.HasIndex("AnswerListId");

                b.ToTable("loinc_answer_list", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincAnswerLink", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("AnswerListId")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("ApplicableContext")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("LinkType")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("LoincNum")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("LoincNum", "AnswerListId")
                    .IsUnique();

                b.ToTable("loinc_answer_link", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincAlias", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Alias")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("LoincNum")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("LoincNum", "Alias")
                    .IsUnique();

                b.ToTable("loinc_alias", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincPanel", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .HasMaxLength(500)
                    .HasColumnType("nvarchar(500)");

                b.Property<string>("PanelLoincNum")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("PanelLoincNum")
                    .IsUnique();

                b.ToTable("loinc_panel", "catalog");
            });

            modelBuilder.Entity("MedicalCatalogService.Models.LoincPanelItem", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("ItemLoincNum")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<string>("Optionality")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("PanelLoincNum")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.HasKey("Id");

                b.HasIndex("PanelLoincNum", "ItemLoincNum")
                    .IsUnique();

                b.ToTable("loinc_panel_item", "catalog");
            });
#pragma warning restore 612, 618
        }
    }
}
