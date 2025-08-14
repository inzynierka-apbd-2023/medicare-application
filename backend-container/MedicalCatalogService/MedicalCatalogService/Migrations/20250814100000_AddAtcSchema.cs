using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Migrations
{
    [Migration("20250814100000_AddAtcSchema")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class AddAtcSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "catalog");
            migrationBuilder.Sql(@"
IF OBJECT_ID('catalog.atc','U') IS NULL
BEGIN
    CREATE TABLE [catalog].[atc](
        [AtcCode] NVARCHAR(10) NOT NULL CONSTRAINT [PK_atc] PRIMARY KEY,
        [AtcName] NVARCHAR(500) NOT NULL,
        [Ddd] DECIMAL(18,4) NULL,
        [Uom] NVARCHAR(50) NULL,
        [AdmR] NVARCHAR(10) NULL,
        [Note] NVARCHAR(1000) NULL
    );
    CREATE INDEX [IX_atc_AtcName] ON [catalog].[atc]([AtcName]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('catalog.atc','U') IS NOT NULL DROP TABLE [catalog].[atc];");
        }
    }
}
