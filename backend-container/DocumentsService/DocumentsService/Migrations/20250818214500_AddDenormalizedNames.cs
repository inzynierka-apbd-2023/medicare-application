using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DocumentsService.Data;

#nullable disable

namespace DocumentsService.Migrations
{
    [Migration("20250818214500_AddDenormalizedNames")]
    [DbContext(typeof(DocumentsDbContext))]
    public partial class AddDenormalizedNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                schema: "documents",
                table: "Document",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                schema: "documents",
                table: "Document",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientName",
                schema: "documents",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                schema: "documents",
                table: "Document");
        }
    }
}
