using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppointmentService.Migrations
{
    public partial class AddUpcomingNotificationSentAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "appointment");
            migrationBuilder.AddColumn<DateTime>(
                name: "UpcomingNotificationSentAt",
                schema: "appointment",
                table: "Appointment",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpcomingNotificationSentAt",
                schema: "appointment",
                table: "Appointment");
        }
    }
}
