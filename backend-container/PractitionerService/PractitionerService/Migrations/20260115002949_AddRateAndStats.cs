using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractitionerService.Migrations
{
    /// <inheritdoc />
    public partial class AddRateAndStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorStatistics",
                schema: "practitioner",
                columns: table => new
                {
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAppointments = table.Column<int>(type: "int", nullable: false),
                    CompletedAppointments = table.Column<int>(type: "int", nullable: false),
                    TotalRatingSum = table.Column<int>(type: "int", nullable: false),
                    TotalRatingCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorStatistics", x => x.DoctorId);
                });

            migrationBuilder.CreateTable(
                name: "Rate",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Rate_Value = table.Column<byte>(type: "tinyint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Patient_User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Doctor_User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Appointment_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rated_At = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Is_Anonymous = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rate_Doctor_User_Id",
                schema: "practitioner",
                table: "Rate",
                column: "Doctor_User_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorStatistics",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Rate",
                schema: "practitioner");
        }
    }
}
