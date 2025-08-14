using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250814101500_WidenAtcAdmR")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class WidenAtcAdmR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('catalog.atc','AdmR') IS NOT NULL
BEGIN
    ALTER TABLE [catalog].[atc] ALTER COLUMN [AdmR] NVARCHAR(50) NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('catalog.atc','AdmR') IS NOT NULL
BEGIN
    ALTER TABLE [catalog].[atc] ALTER COLUMN [AdmR] NVARCHAR(10) NULL;
END
");
        }
    }
}
