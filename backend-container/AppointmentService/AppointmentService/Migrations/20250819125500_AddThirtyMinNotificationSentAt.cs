using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AppointmentService.Data;

namespace AppointmentService.Migrations
{
    [DbContext(typeof(AppointmentDbContext))]
    [Migration("20250819125500_AddThirtyMinNotificationSentAt")]
    public partial class AddThirtyMinNotificationSentAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "appointment");
            migrationBuilder.AddColumn<DateTime>(
                name: "ThirtyMinNotificationSentAt",
                schema: "appointment",
                table: "Appointment",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThirtyMinNotificationSentAt",
                schema: "appointment",
                table: "Appointment");
        }
    }
}
