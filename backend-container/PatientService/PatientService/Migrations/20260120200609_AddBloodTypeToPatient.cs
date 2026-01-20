using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodTypeToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                schema: "patient",
                table: "Patient",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodType",
                schema: "patient",
                table: "Patient");
        }
    }
}
