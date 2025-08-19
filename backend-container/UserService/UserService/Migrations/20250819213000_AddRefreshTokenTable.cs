using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Refresh_Token",
                schema: "user",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    User_Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token_Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Expires_At = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(day,7,SYSUTCDATETIME())"),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Revoked_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Replaced_By_Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Created_By_Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Revoked_By_Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    User_Agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refresh_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refresh_Token_User_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "user",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Refresh_Token_User_Id_Expires_At",
                schema: "user",
                table: "Refresh_Token",
                columns: new[] { "User_Id", "Expires_At" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Refresh_Token",
                schema: "user");
        }
    }
}
