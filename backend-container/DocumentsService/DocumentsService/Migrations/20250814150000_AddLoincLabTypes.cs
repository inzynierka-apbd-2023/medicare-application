using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DocumentsService.Data;

namespace DocumentsService.Migrations
{
    [DbContext(typeof(DocumentsDbContext))]
    [Migration("20250814150000_AddLoincLabTypes")]
    public partial class AddLoincLabTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Lab_Test_Type if it doesn't exist
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[documents].[Lab_Test_Type]', N'U') IS NULL
BEGIN
    CREATE TABLE [documents].[Lab_Test_Type](
        [Id] nvarchar(36) NOT NULL DEFAULT (CONVERT(VARCHAR(36), NEWID())),
        [LoincCode] nvarchar(20) NOT NULL,
        [Name] nvarchar(500) NULL,
        [LoincComponent] nvarchar(255) NULL,
        [LoincProperty] nvarchar(50) NULL,
        [LoincTime] nvarchar(50) NULL,
        [LoincSystem] nvarchar(512) NULL,
        [LoincScale] nvarchar(50) NULL,
        [LoincMethod] nvarchar(100) NULL,
        [ExampleUnits] nvarchar(100) NULL,
        [ReferenceRange] nvarchar(200) NULL,
        CONSTRAINT [PK_Lab_Test_Type] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Lab_Test_Type_LoincCode' AND object_id = OBJECT_ID(N'[documents].[Lab_Test_Type]'))
BEGIN
    CREATE UNIQUE INDEX [IX_Lab_Test_Type_LoincCode] ON [documents].[Lab_Test_Type]([LoincCode]);
END;
");

            // Add LoincCode column to Lab_Test_Result if missing
            migrationBuilder.Sql(@"
IF COL_LENGTH('documents.Lab_Test_Result','LoincCode') IS NULL
BEGIN
    ALTER TABLE [documents].[Lab_Test_Result] ADD [LoincCode] nvarchar(20) NULL;
END;
");

            // Ensure index and FK on existing LabTestTypeId column
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Lab_Test_Result_LabTestTypeId' AND object_id = OBJECT_ID(N'[documents].[Lab_Test_Result]'))
BEGIN
    CREATE INDEX [IX_Lab_Test_Result_LabTestTypeId] ON [documents].[Lab_Test_Result]([LabTestTypeId]);
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Lab_Test_Result_Lab_Test_Type_LabTestTypeId')
BEGIN
    ALTER TABLE [documents].[Lab_Test_Result] WITH NOCHECK
    ADD CONSTRAINT [FK_Lab_Test_Result_Lab_Test_Type_LabTestTypeId]
    FOREIGN KEY([LabTestTypeId]) REFERENCES [documents].[Lab_Test_Type]([Id]) ON DELETE NO ACTION;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Lab_Test_Result_Lab_Test_Type_LabTestTypeId')
BEGIN
    ALTER TABLE [documents].[Lab_Test_Result] DROP CONSTRAINT [FK_Lab_Test_Result_Lab_Test_Type_LabTestTypeId];
END;
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Lab_Test_Result_LabTestTypeId' AND object_id = OBJECT_ID(N'[documents].[Lab_Test_Result]'))
BEGIN
    DROP INDEX [IX_Lab_Test_Result_LabTestTypeId] ON [documents].[Lab_Test_Result];
END;
IF COL_LENGTH('documents.Lab_Test_Result','LoincCode') IS NOT NULL
BEGIN
    ALTER TABLE [documents].[Lab_Test_Result] DROP COLUMN [LoincCode];
END;
IF OBJECT_ID(N'[documents].[Lab_Test_Type]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [documents].[Lab_Test_Type];
END;
");
        }
    }
}
