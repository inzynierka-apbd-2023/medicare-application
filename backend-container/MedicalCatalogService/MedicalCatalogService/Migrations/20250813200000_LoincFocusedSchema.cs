using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813200000_LoincFocusedSchema")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class LoincFocusedSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "catalog");

            // Drop legacy/unneeded tables if they exist
            migrationBuilder.Sql("IF OBJECT_ID('catalog.mappings','U') IS NOT NULL DROP TABLE catalog.mappings;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.snomed','U') IS NOT NULL DROP TABLE catalog.snomed;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.cpt','U') IS NOT NULL DROP TABLE catalog.cpt;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.hcpcs','U') IS NOT NULL DROP TABLE catalog.hcpcs;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.Medical_Condition','U') IS NOT NULL DROP TABLE catalog.Medical_Condition;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.Lab_Test_Type','U') IS NOT NULL DROP TABLE catalog.Lab_Test_Type;");


            // Create new LOINC-focused tables (if not exists)
            migrationBuilder.Sql(@"
IF OBJECT_ID('catalog.loinc','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc](
        [LoincNum] NVARCHAR(20) NOT NULL CONSTRAINT [PK_loinc] PRIMARY KEY,
        [LongCommonName] NVARCHAR(500) NULL,
        [ShortName] NVARCHAR(255) NULL,
        [Component] NVARCHAR(255) NULL,
        [Property] NVARCHAR(50) NULL,
        [TimeAspect] NVARCHAR(50) NULL,
        [System] NVARCHAR(100) NULL,
        [ScaleType] NVARCHAR(50) NULL,
        [MethodType] NVARCHAR(100) NULL,
        [Class] NVARCHAR(100) NULL,
        [Status] NVARCHAR(50) NULL,
        [VersionLastChanged] NVARCHAR(50) NULL,
        [DefinitionDescription] NVARCHAR(2000) NULL,
        [ExampleUnits] NVARCHAR(100) NULL,
        [ExternalCopyrightNotice] NVARCHAR(1000) NULL,
        [PanelType] NVARCHAR(50) NULL,
        [Equation] NVARCHAR(2000) NULL
    );
    CREATE INDEX [IX_loinc_Component] ON [catalog].[loinc]([Component]);
    CREATE INDEX [IX_loinc_LongCommonName] ON [catalog].[loinc]([LongCommonName]);
    CREATE INDEX [IX_loinc_ShortName] ON [catalog].[loinc]([ShortName]);
END

IF OBJECT_ID('catalog.loinc_map_to','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_map_to](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_map_to] PRIMARY KEY,
        [FromLoinc] NVARCHAR(20) NOT NULL,
        [ToLoinc] NVARCHAR(20) NOT NULL,
        [MapType] NVARCHAR(50) NULL,
        [Comment] NVARCHAR(500) NULL
    );
    CREATE UNIQUE INDEX [IX_loinc_map_to_From_To] ON [catalog].[loinc_map_to]([FromLoinc],[ToLoinc]);
END

IF OBJECT_ID('catalog.loinc_answer_list','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_answer_list](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_answer_list] PRIMARY KEY,
        [AnswerListId] NVARCHAR(50) NOT NULL,
        [AnswerStringId] NVARCHAR(50) NULL,
        [DisplayName] NVARCHAR(255) NULL,
        [Description] NVARCHAR(1000) NULL
    );
    CREATE INDEX [IX_loinc_answer_list_AnswerListId] ON [catalog].[loinc_answer_list]([AnswerListId]);
END

IF OBJECT_ID('catalog.loinc_answer_link','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_answer_link](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_answer_link] PRIMARY KEY,
        [LoincNum] NVARCHAR(20) NOT NULL,
        [AnswerListId] NVARCHAR(50) NOT NULL,
        [LinkType] NVARCHAR(50) NULL
    );
    CREATE UNIQUE INDEX [IX_loinc_answer_link_Loinc_List] ON [catalog].[loinc_answer_link]([LoincNum],[AnswerListId]);
END

IF OBJECT_ID('catalog.loinc_panel','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_panel](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_panel] PRIMARY KEY,
        [PanelLoincNum] NVARCHAR(20) NOT NULL
    );
    CREATE UNIQUE INDEX [IX_loinc_panel_Panel] ON [catalog].[loinc_panel]([PanelLoincNum]);
END

IF OBJECT_ID('catalog.loinc_panel_item','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_panel_item](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_panel_item] PRIMARY KEY,
        [PanelLoincNum] NVARCHAR(20) NOT NULL,
        [ItemLoincNum] NVARCHAR(20) NOT NULL,
        [Ordinal] INT NULL,
        [Optionality] NVARCHAR(50) NULL
    );
    CREATE UNIQUE INDEX [IX_loinc_panel_item_Panel_Item] ON [catalog].[loinc_panel_item]([PanelLoincNum],[ItemLoincNum]);
END

IF OBJECT_ID('catalog.loinc_consumer_name','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[loinc_consumer_name](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_consumer_name] PRIMARY KEY,
        [LoincNum] NVARCHAR(20) NOT NULL,
        [ConsumerName] NVARCHAR(255) NOT NULL,
        [Language] NVARCHAR(20) NULL
    );
    CREATE UNIQUE INDEX [IX_loinc_consumer_name_key] ON [catalog].[loinc_consumer_name]([LoincNum],[ConsumerName],[Language]);
END

-- Ensure release table has Description column (if release table exists)
IF OBJECT_ID('catalog.release','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('catalog.release','Description') IS NULL
    BEGIN
        ALTER TABLE [catalog].[release] ADD [Description] NVARCHAR(200) NULL;
    END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new tables
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_consumer_name','U') IS NOT NULL DROP TABLE catalog.loinc_consumer_name;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_panel_item','U') IS NOT NULL DROP TABLE catalog.loinc_panel_item;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_panel','U') IS NOT NULL DROP TABLE catalog.loinc_panel;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_answer_link','U') IS NOT NULL DROP TABLE catalog.loinc_answer_link;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_answer_list','U') IS NOT NULL DROP TABLE catalog.loinc_answer_list;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc_map_to','U') IS NOT NULL DROP TABLE catalog.loinc_map_to;");
            migrationBuilder.Sql("IF OBJECT_ID('catalog.loinc','U') IS NOT NULL DROP TABLE catalog.loinc;");

            // Optionally recreate legacy 'mappings' table as empty to allow Down migration
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
    }
}
