using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PatientService.Data;

namespace PatientService.Migrations
{
    [Migration("20250818110000_AddPatientStatusIdempotency")]
    [DbContext(typeof(PatientDbContext))]
    public partial class AddPatientStatusIdempotency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "patient",
                table: "Patient_Status",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_Status_IdempotencyKey",
                schema: "patient",
                table: "Patient_Status",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patient_Status_IdempotencyKey",
                schema: "patient",
                table: "Patient_Status");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "patient",
                table: "Patient_Status");
        }
    }
}
