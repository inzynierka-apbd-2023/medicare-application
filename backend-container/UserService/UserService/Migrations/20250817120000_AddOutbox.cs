using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UserService.Data;

namespace UserService.Migrations
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20250817120000_AddOutbox")]
    public partial class AddOutbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "user");
            migrationBuilder.CreateTable(
                name: "Outbox_Event",
                schema: "user",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Outbox_Event", x => x.Id); }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Event_PublishedAt",
                schema: "user",
                table: "Outbox_Event",
                column: "PublishedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Outbox_Event", schema: "user");
        }
    }
}
