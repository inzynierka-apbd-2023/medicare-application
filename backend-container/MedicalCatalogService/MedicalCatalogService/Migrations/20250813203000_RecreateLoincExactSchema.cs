using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813203000_RecreateLoincExactSchema")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class RecreateLoincExactSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "catalog");

            // Drop existing LOINC-related tables unconditionally to align exact schema
            migrationBuilder.Sql(@"
IF OBJECT_ID('catalog.loinc_answer_link','U') IS NOT NULL DROP TABLE catalog.loinc_answer_link;
IF OBJECT_ID('catalog.loinc_answer_list','U') IS NOT NULL DROP TABLE catalog.loinc_answer_list;
IF OBJECT_ID('catalog.loinc_panel_item','U') IS NOT NULL DROP TABLE catalog.loinc_panel_item;
IF OBJECT_ID('catalog.loinc_panel','U') IS NOT NULL DROP TABLE catalog.loinc_panel;
IF OBJECT_ID('catalog.loinc_map_to','U') IS NOT NULL DROP TABLE catalog.loinc_map_to;
IF OBJECT_ID('catalog.loinc_consumer_name','U') IS NOT NULL DROP TABLE catalog.loinc_consumer_name;
IF OBJECT_ID('catalog.loinc','U') IS NOT NULL DROP TABLE catalog.loinc;
");

            // Recreate exact schema (non-fulltext DDL inside transaction)
            migrationBuilder.Sql(@"
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

CREATE TABLE [catalog].[loinc_map_to](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_map_to] PRIMARY KEY,
    [FromLoinc] NVARCHAR(20) NOT NULL,
    [ToLoinc] NVARCHAR(20) NOT NULL,
    [MapType] NVARCHAR(50) NULL,
    [Comment] NVARCHAR(500) NULL
);
CREATE UNIQUE INDEX [IX_loinc_map_to_From_To] ON [catalog].[loinc_map_to]([FromLoinc],[ToLoinc]);

CREATE TABLE [catalog].[loinc_answer_list](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_answer_list] PRIMARY KEY,
    [AnswerListId] NVARCHAR(50) NOT NULL,
    [AnswerStringId] NVARCHAR(50) NULL,
    [DisplayName] NVARCHAR(255) NULL,
    [Description] NVARCHAR(1000) NULL
);
CREATE INDEX [IX_loinc_answer_list_AnswerListId] ON [catalog].[loinc_answer_list]([AnswerListId]);

CREATE TABLE [catalog].[loinc_answer_link](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_answer_link] PRIMARY KEY,
    [LoincNum] NVARCHAR(20) NOT NULL,
    [AnswerListId] NVARCHAR(50) NOT NULL,
    [LinkType] NVARCHAR(50) NULL
);
CREATE UNIQUE INDEX [IX_loinc_answer_link_Loinc_List] ON [catalog].[loinc_answer_link]([LoincNum],[AnswerListId]);

CREATE TABLE [catalog].[loinc_panel](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_panel] PRIMARY KEY,
    [PanelLoincNum] NVARCHAR(20) NOT NULL
);
CREATE UNIQUE INDEX [IX_loinc_panel_Panel] ON [catalog].[loinc_panel]([PanelLoincNum]);

CREATE TABLE [catalog].[loinc_panel_item](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_panel_item] PRIMARY KEY,
    [PanelLoincNum] NVARCHAR(20) NOT NULL,
    [ItemLoincNum] NVARCHAR(20) NOT NULL,
    [Ordinal] INT NULL,
    [Optionality] NVARCHAR(50) NULL
);
CREATE UNIQUE INDEX [IX_loinc_panel_item_Panel_Item] ON [catalog].[loinc_panel_item]([PanelLoincNum],[ItemLoincNum]);

CREATE TABLE [catalog].[loinc_consumer_name](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_loinc_consumer_name] PRIMARY KEY,
    [LoincNum] NVARCHAR(20) NOT NULL,
    [ConsumerName] NVARCHAR(255) NOT NULL,
    [Language] NVARCHAR(20) NULL
);
CREATE UNIQUE INDEX [IX_loinc_consumer_name_key] ON [catalog].[loinc_consumer_name]([LoincNum],[ConsumerName],[Language]);
");

            // Full-text index must be created outside a transaction
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'ftCatalog')
BEGIN
    CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;
END
", suppressTransaction: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('catalog.loinc'))
BEGIN
    CREATE FULLTEXT INDEX ON [catalog].[loinc]([LongCommonName] LANGUAGE 1033, [Component] LANGUAGE 1033, [ShortName] LANGUAGE 1033) KEY INDEX [PK_loinc];
END
", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op rollback (intentionally left empty for destructive reset)
        }
    }
}
