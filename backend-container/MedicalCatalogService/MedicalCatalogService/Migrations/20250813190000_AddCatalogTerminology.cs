using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813190000_AddCatalogTerminology")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class AddCatalogTerminology : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "catalog");

            migrationBuilder.CreateTable(
                name: "icd10",
                schema: "catalog",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_icd10", x => x.Code); }
            );
            migrationBuilder.CreateIndex(name: "IX_icd10_Title", schema: "catalog", table: "icd10", column: "Title");

            migrationBuilder.CreateTable(
                name: "snomed",
                schema: "catalog",
                columns: table => new
                {
                    ConceptId = table.Column<long>(type: "bigint", nullable: false),
                    Fsn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredTerm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_snomed", x => x.ConceptId); }
            );
            migrationBuilder.CreateIndex(name: "IX_snomed_Active", schema: "catalog", table: "snomed", column: "Active");
            migrationBuilder.CreateIndex(name: "IX_snomed_PreferredTerm", schema: "catalog", table: "snomed", column: "PreferredTerm");

            migrationBuilder.CreateTable(
                name: "loinc",
                schema: "catalog",
                columns: table => new
                {
                    LoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Component = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Property = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TimeAspct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    System = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScaleTyp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MethodTyp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LongCommonName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_loinc", x => x.LoincNum); }
            );
            migrationBuilder.CreateIndex(name: "IX_loinc_Component", schema: "catalog", table: "loinc", column: "Component");
            migrationBuilder.CreateIndex(name: "IX_loinc_LongCommonName", schema: "catalog", table: "loinc", column: "LongCommonName");

            migrationBuilder.CreateTable(
                name: "cpt",
                schema: "catalog",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShortDesc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LongDesc = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_cpt", x => x.Code); }
            );

            migrationBuilder.CreateTable(
                name: "hcpcs",
                schema: "catalog",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShortDesc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LongDesc = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ModifierFlags = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_hcpcs", x => x.Code); }
            );

            migrationBuilder.CreateTable(
                name: "release",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    System = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReleasedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_release", x => x.Id); }
            );
            migrationBuilder.CreateIndex(name: "IX_release_System_Version", schema: "catalog", table: "release", columns: new[] { "System", "Version" }, unique: true);

            migrationBuilder.CreateTable(
                name: "mappings",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Confidence = table.Column<double>(type: "float", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_mappings", x => x.Id); }
            );
            migrationBuilder.CreateIndex(name: "IX_mappings_Source", schema: "catalog", table: "mappings", columns: new[] { "SourceSystem", "SourceCode" });
            migrationBuilder.CreateIndex(name: "IX_mappings_Target", schema: "catalog", table: "mappings", columns: new[] { "TargetSystem", "TargetCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "mappings", schema: "catalog");
            migrationBuilder.DropTable(name: "release", schema: "catalog");
            migrationBuilder.DropTable(name: "hcpcs", schema: "catalog");
            migrationBuilder.DropTable(name: "cpt", schema: "catalog");
            migrationBuilder.DropTable(name: "loinc", schema: "catalog");
            migrationBuilder.DropTable(name: "snomed", schema: "catalog");
            migrationBuilder.DropTable(name: "icd10", schema: "catalog");
        }
    }
}
