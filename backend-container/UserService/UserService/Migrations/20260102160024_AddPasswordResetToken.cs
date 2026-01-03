using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Password_Reset_Token",
                schema: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token_Hash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Expires_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Used_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Password_Reset_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Password_Reset_Token_User_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "user",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Password_Reset_Token_Token_Hash",
                schema: "user",
                table: "Password_Reset_Token",
                column: "Token_Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Password_Reset_Token_User_Id",
                schema: "user",
                table: "Password_Reset_Token",
                column: "User_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Password_Reset_Token",
                schema: "user");
        }
    }
}
