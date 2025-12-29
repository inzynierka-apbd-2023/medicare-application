using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractitionerService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "practitioner");

            migrationBuilder.CreateTable(
                name: "Doctor",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Receptionist",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receptionist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specialization",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doctor_Schedule",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_Schedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctor_Schedule_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "practitioner",
                        principalTable: "Doctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctor_Specialization",
                schema: "practitioner",
                columns: table => new
                {
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_Specialization", x => new { x.DoctorId, x.SpecializationId });
                    table.ForeignKey(
                        name: "FK_Doctor_Specialization_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "practitioner",
                        principalTable: "Doctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Doctor_Specialization_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalSchema: "practitioner",
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specialization_Service",
                schema: "practitioner",
                columns: table => new
                {
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization_Service", x => new { x.SpecializationId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_Specialization_Service_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "practitioner",
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Specialization_Service_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalSchema: "practitioner",
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_UserId",
                schema: "practitioner",
                table: "Doctor",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Schedule_DoctorId",
                schema: "practitioner",
                table: "Doctor_Schedule",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Specialization_SpecializationId",
                schema: "practitioner",
                table: "Doctor_Specialization",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Receptionist_UserId",
                schema: "practitioner",
                table: "Receptionist",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specialization_Service_ServiceId",
                schema: "practitioner",
                table: "Specialization_Service",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doctor_Schedule",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Doctor_Specialization",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Receptionist",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Specialization_Service",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Doctor",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Service",
                schema: "practitioner");

            migrationBuilder.DropTable(
                name: "Specialization",
                schema: "practitioner");
        }
    }
}
