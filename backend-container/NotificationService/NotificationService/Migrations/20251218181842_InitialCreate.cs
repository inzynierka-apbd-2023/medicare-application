using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.CreateTable(
                name: "Notification",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", nullable: false),
                    Recipient_User_Id = table.Column<string>(type: "varchar(36)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Creation_Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Source_Service = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    Is_Read = table.Column<bool>(type: "bit", nullable: true),
                    Action_Url = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Priority_Level = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Expires_At = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification",
                schema: "notifications");
        }
    }
}
