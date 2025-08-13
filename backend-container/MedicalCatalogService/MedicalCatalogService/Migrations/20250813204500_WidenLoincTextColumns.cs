using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813204500_WidenLoincTextColumns")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class WidenLoincTextColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [catalog].[loinc] ALTER COLUMN [DefinitionDescription] NVARCHAR(MAX) NULL;
ALTER TABLE [catalog].[loinc] ALTER COLUMN [ExternalCopyrightNotice] NVARCHAR(MAX) NULL;
ALTER TABLE [catalog].[loinc] ALTER COLUMN [Equation] NVARCHAR(MAX) NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [catalog].[loinc] ALTER COLUMN [DefinitionDescription] NVARCHAR(2000) NULL;
ALTER TABLE [catalog].[loinc] ALTER COLUMN [ExternalCopyrightNotice] NVARCHAR(1000) NULL;
ALTER TABLE [catalog].[loinc] ALTER COLUMN [Equation] NVARCHAR(2000) NULL;
");
        }
    }
}
