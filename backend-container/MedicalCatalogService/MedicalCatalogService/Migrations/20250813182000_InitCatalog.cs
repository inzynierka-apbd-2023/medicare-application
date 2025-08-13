using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MedicalCatalogService.Data;

#nullable disable

namespace MedicalCatalogService.Migrations
{
    [Migration("20250813182000_InitCatalog")]
    [DbContext(typeof(MedicalCatalogDbContext))]
    public partial class InitCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "catalog");

            migrationBuilder.CreateTable(
                name: "Medical_Condition",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medical_Condition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Condition_Code",
                schema: "catalog",
                table: "Medical_Condition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateTable(
                name: "Lab_Test_Type",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Test_Type", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_Type_Code",
                schema: "catalog",
                table: "Lab_Test_Type",
                column: "Code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Lab_Test_Type", schema: "catalog");
            migrationBuilder.DropTable(name: "Medical_Condition", schema: "catalog");
        }
    }
}
