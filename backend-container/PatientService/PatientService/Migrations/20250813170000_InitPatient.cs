using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PatientService.Data;

#nullable disable

namespace PatientService.Migrations
{
    [Migration("20250813170000_InitPatient")]
    [DbContext(typeof(PatientDbContext))]
    public partial class InitPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "patient");

            migrationBuilder.CreateTable(
                name: "Patient",
                schema: "patient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PrimaryDoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emergency_Contact",
                schema: "patient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Relation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emergency_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insurance",
                schema: "patient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PolicyNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patient_Status",
                schema: "patient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EffectiveAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient_Status", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_UserId",
                schema: "patient",
                table: "Patient",
                column: "UserId",
                unique: true);

            migrationBuilder.Sql(@"CREATE OR ALTER VIEW patient.PatientOverview AS
SELECT p.Id AS PatientId,
       p.UserId,
       up.FirstName,
       up.LastName,
       up.Email,
       up.Phone,
       (SELECT TOP 1 s.Status FROM patient.Patient_Status s WHERE s.PatientId = p.Id ORDER BY s.EffectiveAt DESC) AS CurrentStatus,
       (SELECT TOP 1 ec.Name FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactName,
       (SELECT TOP 1 ec.Phone FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactPhone
FROM patient.Patient p
LEFT JOIN [user].[User_Profile] up ON up.User_Id = p.UserId;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('patient.PatientOverview','V') IS NOT NULL DROP VIEW patient.PatientOverview;");
            migrationBuilder.DropTable(name: "Patient_Status", schema: "patient");
            migrationBuilder.DropTable(name: "Insurance", schema: "patient");
            migrationBuilder.DropTable(name: "Emergency_Contact", schema: "patient");
            migrationBuilder.DropTable(name: "Patient", schema: "patient");
        }
    }
}
