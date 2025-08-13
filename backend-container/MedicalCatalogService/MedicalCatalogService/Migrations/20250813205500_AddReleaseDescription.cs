using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813205500_AddReleaseDescription")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class AddReleaseDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF COL_LENGTH('catalog.release','Description') IS NULL ALTER TABLE [catalog].[release] ADD [Description] NVARCHAR(200) NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no down migration to avoid data loss
        }
    }
}
