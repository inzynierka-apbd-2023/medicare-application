using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813205000_WidenLoincSystem")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class WidenLoincSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE [catalog].[loinc] ALTER COLUMN [System] NVARCHAR(512) NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE [catalog].[loinc] ALTER COLUMN [System] NVARCHAR(100) NULL;");
        }
    }
}
