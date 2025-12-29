using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCatalogService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "atc",
                schema: "catalog",
                columns: table => new
                {
                    AtcCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AtcName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ddd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Uom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdmR = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atc", x => x.AtcCode);
                });

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
                constraints: table =>
                {
                    table.PrimaryKey("PK_icd10", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "loinc",
                schema: "catalog",
                columns: table => new
                {
                    LoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LongCommonName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Component = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Property = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TimeAspect = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    System = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScaleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MethodType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Class = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VersionLastChanged = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefinitionDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExampleUnits = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalCopyrightNotice = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PanelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Equation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc", x => x.LoincNum);
                });

            migrationBuilder.CreateTable(
                name: "loinc_answer_link",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnswerListId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_answer_link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loinc_answer_list",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AnswerListId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnswerStringId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_answer_list", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loinc_consumer_name",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_consumer_name", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loinc_map_to",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FromLoinc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToLoinc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MapType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_map_to", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loinc_panel",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PanelLoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_panel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loinc_panel_item",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PanelLoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemLoincNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: true),
                    Optionality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loinc_panel_item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "release",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    System = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReleasedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_release", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atc_AtcName",
                schema: "catalog",
                table: "atc",
                column: "AtcName");

            migrationBuilder.CreateIndex(
                name: "IX_icd10_Title",
                schema: "catalog",
                table: "icd10",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_Component",
                schema: "catalog",
                table: "loinc",
                column: "Component");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_LongCommonName",
                schema: "catalog",
                table: "loinc",
                column: "LongCommonName");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_ShortName",
                schema: "catalog",
                table: "loinc",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_answer_link_LoincNum_AnswerListId",
                schema: "catalog",
                table: "loinc_answer_link",
                columns: new[] { "LoincNum", "AnswerListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loinc_answer_list_AnswerListId",
                schema: "catalog",
                table: "loinc_answer_list",
                column: "AnswerListId");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_consumer_name_LoincNum_ConsumerName_Language",
                schema: "catalog",
                table: "loinc_consumer_name",
                columns: new[] { "LoincNum", "ConsumerName", "Language" },
                unique: true,
                filter: "[Language] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_loinc_map_to_FromLoinc_ToLoinc",
                schema: "catalog",
                table: "loinc_map_to",
                columns: new[] { "FromLoinc", "ToLoinc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loinc_panel_PanelLoincNum",
                schema: "catalog",
                table: "loinc_panel",
                column: "PanelLoincNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loinc_panel_item_PanelLoincNum_ItemLoincNum",
                schema: "catalog",
                table: "loinc_panel_item",
                columns: new[] { "PanelLoincNum", "ItemLoincNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_release_System_Version",
                schema: "catalog",
                table: "release",
                columns: new[] { "System", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atc",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "icd10",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_answer_link",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_answer_list",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_consumer_name",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_map_to",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_panel",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "loinc_panel_item",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "release",
                schema: "catalog");
        }
    }
}
