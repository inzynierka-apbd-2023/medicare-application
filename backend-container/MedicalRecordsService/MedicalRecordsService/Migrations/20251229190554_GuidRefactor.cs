using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalRecordsService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "medical");

            migrationBuilder.CreateTable(
                name: "Diagnosis",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icd10Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Primary"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medical_Record",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HistoryOfPresentIllness = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhysicalExamination = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Assessment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Plan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medical_Record", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AtcCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrescribedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vital_Signs",
                schema: "medical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    SystolicBP = table.Column<int>(type: "int", nullable: true),
                    DiastolicBP = table.Column<int>(type: "int", nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "int", nullable: true),
                    OxygenSaturation = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vital_Signs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosis_Icd10Code",
                schema: "medical",
                table: "Diagnosis",
                column: "Icd10Code");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosis_MedicalRecordId",
                schema: "medical",
                table: "Diagnosis",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_DoctorId",
                schema: "medical",
                table: "Medical_Record",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_PatientId",
                schema: "medical",
                table: "Medical_Record",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Record_VisitDate",
                schema: "medical",
                table: "Medical_Record",
                column: "VisitDate");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_MedicalRecordId",
                schema: "medical",
                table: "Prescription",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_PatientId",
                schema: "medical",
                table: "Prescription",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Vital_Signs_MeasuredAt",
                schema: "medical",
                table: "Vital_Signs",
                column: "MeasuredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Vital_Signs_MedicalRecordId",
                schema: "medical",
                table: "Vital_Signs",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Vital_Signs_PatientId",
                schema: "medical",
                table: "Vital_Signs",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diagnosis",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "Medical_Record",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "Prescription",
                schema: "medical");

            migrationBuilder.DropTable(
                name: "Vital_Signs",
                schema: "medical");
        }
    }
}
