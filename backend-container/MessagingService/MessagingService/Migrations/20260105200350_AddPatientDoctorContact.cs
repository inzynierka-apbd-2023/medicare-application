using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessagingService.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientDoctorContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patient_Doctor_Contact",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoctorSpecialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FirstContactAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastContactAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient_Doctor_Contact", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_Doctor_Contact_DoctorUserId",
                schema: "messaging",
                table: "Patient_Doctor_Contact",
                column: "DoctorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_Doctor_Contact_PatientUserId",
                schema: "messaging",
                table: "Patient_Doctor_Contact",
                column: "PatientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_Doctor_Contact_PatientUserId_DoctorUserId",
                schema: "messaging",
                table: "Patient_Doctor_Contact",
                columns: new[] { "PatientUserId", "DoctorUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patient_Doctor_Contact",
                schema: "messaging");
        }
    }
}
