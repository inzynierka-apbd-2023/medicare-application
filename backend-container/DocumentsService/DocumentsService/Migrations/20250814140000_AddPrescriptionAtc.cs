using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DocumentsService.Data;

namespace DocumentsService.Migrations
{
    [DbContext(typeof(DocumentsDbContext))]
    [Migration("20250814140000_AddPrescriptionAtc")]
    public partial class AddPrescriptionAtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AtcCode",
                schema: "documents",
                table: "Prescription",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AtcName",
                schema: "documents",
                table: "Prescription",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_AtcCode",
                schema: "documents",
                table: "Prescription",
                column: "AtcCode");

            migrationBuilder.AlterColumn<decimal>(
                name: "NumericValue",
                schema: "documents",
                table: "Lab_Test_Result",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prescription_AtcCode",
                schema: "documents",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "AtcCode",
                schema: "documents",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "AtcName",
                schema: "documents",
                table: "Prescription");
        }
    }
}
