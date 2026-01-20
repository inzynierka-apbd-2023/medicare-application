using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Migrations
{
    /// <inheritdoc />
    public partial class AddMassTransitOutboxSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OutboxState",
                newName: "OutboxState",
                newSchema: "patient");

            migrationBuilder.RenameTable(
                name: "OutboxMessage",
                newName: "OutboxMessage",
                newSchema: "patient");

            migrationBuilder.RenameTable(
                name: "InboxState",
                newName: "InboxState",
                newSchema: "patient");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OutboxState",
                schema: "patient",
                newName: "OutboxState");

            migrationBuilder.RenameTable(
                name: "OutboxMessage",
                schema: "patient",
                newName: "OutboxMessage");

            migrationBuilder.RenameTable(
                name: "InboxState",
                schema: "patient",
                newName: "InboxState");
        }
    }
}
