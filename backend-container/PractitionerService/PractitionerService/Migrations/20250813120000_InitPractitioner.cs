using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

#nullable disable

namespace PractitionerService.Migrations
{
    [Migration("20250813120000_InitPractitioner")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class InitPractitioner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "practitioner");

            migrationBuilder.CreateTable(
                name: "Doctor",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
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
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
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
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doctor_Specialization",
                schema: "practitioner",
                columns: table => new
                {
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SpecializationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_Specialization", x => new { x.DoctorId, x.SpecializationId });
                });

            migrationBuilder.CreateTable(
                name: "Doctor_Schedule",
                schema: "practitioner",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_Schedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_UserId",
                schema: "practitioner",
                table: "Doctor",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receptionist_UserId",
                schema: "practitioner",
                table: "Receptionist",
                column: "UserId",
                unique: true);

            // Create projection view joining users.User_Profile
            migrationBuilder.Sql(@"IF OBJECT_ID('practitioner.DoctorDirectory', 'V') IS NOT NULL DROP VIEW practitioner.DoctorDirectory;
            EXEC('CREATE VIEW practitioner.DoctorDirectory AS
                SELECT d.Id AS DoctorId, d.UserId, up.FirstName, up.LastName, up.Email, up.Phone,
                    STUFF((SELECT '','' + ds.SpecializationId FROM practitioner.Doctor_Specialization ds WHERE ds.DoctorId = d.Id FOR XML PATH(''''), TYPE).value(''.'', ''NVARCHAR(MAX)''), 1, 1, '''') AS Specializations,
                    NULL AS Services
                FROM practitioner.Doctor d
                LEFT JOIN dbo.[User_Profile] up ON up.User_Id = d.UserId');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('practitioner.DoctorDirectory', 'V') IS NOT NULL DROP VIEW practitioner.DoctorDirectory;");
            migrationBuilder.DropTable(name: "Doctor_Schedule", schema: "practitioner");
            migrationBuilder.DropTable(name: "Doctor_Specialization", schema: "practitioner");
            migrationBuilder.DropTable(name: "Specialization", schema: "practitioner");
            migrationBuilder.DropTable(name: "Service", schema: "practitioner");
            migrationBuilder.DropTable(name: "Receptionist", schema: "practitioner");
            migrationBuilder.DropTable(name: "Doctor", schema: "practitioner");
        }
    }
}
