using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "appointment");

            migrationBuilder.CreateTable(
                name: "Appointment",
                schema: "appointment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Scheduled"),
                    AppointmentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoomId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpcomingNotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThirtyMinNotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointment_Category",
                schema: "appointment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointment_Slot",
                schema: "appointment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AppointmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment_Slot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                schema: "appointment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorId",
                schema: "appointment",
                table: "Appointment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientId",
                schema: "appointment",
                table: "Appointment",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ScheduledAt",
                schema: "appointment",
                table: "Appointment",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Status_ScheduledEndAt",
                schema: "appointment",
                table: "Appointment",
                columns: new[] { "Status", "ScheduledEndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Category_Name",
                schema: "appointment",
                table: "Appointment_Category",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Slot_DoctorId",
                schema: "appointment",
                table: "Appointment_Slot",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Slot_StartTime",
                schema: "appointment",
                table: "Appointment_Slot",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_DoctorId",
                schema: "appointment",
                table: "Schedule",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointment",
                schema: "appointment");

            migrationBuilder.DropTable(
                name: "Appointment_Category",
                schema: "appointment");

            migrationBuilder.DropTable(
                name: "Appointment_Slot",
                schema: "appointment");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "appointment");
        }
    }
}
