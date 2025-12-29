using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessagingService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "Message",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Normal"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Message_Receipt",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message_Receipt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Message_Thread",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message_Thread", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Thread_Message",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thread_Message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Thread_Participant",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thread_Participant", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Message_IsRead",
                schema: "messaging",
                table: "Message",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Message_RecipientId",
                schema: "messaging",
                table: "Message",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SenderId",
                schema: "messaging",
                table: "Message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SentAt",
                schema: "messaging",
                table: "Message",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Message_Receipt_MessageId",
                schema: "messaging",
                table: "Message_Receipt",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_Receipt_MessageId_UserId",
                schema: "messaging",
                table: "Message_Receipt",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Message_Receipt_UserId",
                schema: "messaging",
                table: "Message_Receipt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_Thread_InitiatorId",
                schema: "messaging",
                table: "Message_Thread",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Message_SenderId",
                schema: "messaging",
                table: "Thread_Message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Message_SentAt",
                schema: "messaging",
                table: "Thread_Message",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Message_ThreadId",
                schema: "messaging",
                table: "Thread_Message",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Participant_ThreadId",
                schema: "messaging",
                table: "Thread_Participant",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Participant_ThreadId_UserId",
                schema: "messaging",
                table: "Thread_Participant",
                columns: new[] { "ThreadId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Thread_Participant_UserId",
                schema: "messaging",
                table: "Thread_Participant",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Message",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "Message_Receipt",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "Message_Thread",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "Thread_Message",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "Thread_Participant",
                schema: "messaging");
        }
    }
}
