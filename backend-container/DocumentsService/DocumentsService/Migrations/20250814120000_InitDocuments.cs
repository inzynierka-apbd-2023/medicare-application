using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DocumentsService.Data;

#nullable disable

namespace DocumentsService.Migrations
{
    [Migration("20250814120000_InitDocuments")]
    [DbContext(typeof(DocumentsDbContext))]
    public partial class InitDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "documents");

            migrationBuilder.CreateTable(
                name: "Document_Type",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TemplatePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Document_Type", x => x.Id); }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Document_Type_Code",
                schema: "documents",
                table: "Document_Type",
                column: "Code",
                unique: true
            );

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_Document_Type_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "documents",
                        principalTable: "Document_Type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Document_Patient_Type_Created",
                schema: "documents",
                table: "Document",
                columns: new[] { "PatientId", "Type", "CreatedAt" }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Document_Doctor_Created",
                schema: "documents",
                table: "Document",
                columns: new[] { "DoctorId", "CreatedAt" }
            );

            migrationBuilder.CreateTable(
                name: "Visit_Document",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VitalSignsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visit_Document", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Visit_Document_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );

            migrationBuilder.CreateTable(
                name: "Prescription",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PharmacyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacyPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefillsRemaining = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Prescription_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );

            migrationBuilder.CreateTable(
                name: "Referral",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Speciality = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferredTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referral", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Referral_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );

            migrationBuilder.CreateTable(
                name: "Sick_Leave",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DaysOff = table.Column<int>(type: "int", nullable: true),
                    ReturnToWorkDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkRestrictions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sick_Leave", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Sick_Leave_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );

            migrationBuilder.CreateTable(
                name: "Lab_Results",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Laboratory = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OverallStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Interpretation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceRanges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicianName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoctorComments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Results", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Lab_Results_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );

            migrationBuilder.CreateTable(
                name: "Lab_Test_Result",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    LabResultsDocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    LabTestTypeId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ParameterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Test_Result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lab_Test_Result_Lab_Results_LabResultsDocumentId",
                        column: x => x.LabResultsDocumentId,
                        principalSchema: "documents",
                        principalTable: "Lab_Results",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_Result_LabResultsDocumentId",
                schema: "documents",
                table: "Lab_Test_Result",
                column: "LabResultsDocumentId"
            );

            migrationBuilder.CreateTable(
                name: "Documents_Assigned",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    DocumentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents_Assigned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Assigned_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                }
            );
            migrationBuilder.CreateIndex(
                name: "IX_Documents_Assigned_Document_Appointment",
                schema: "documents",
                table: "Documents_Assigned",
                columns: new[] { "DocumentId", "AppointmentId" },
                unique: true
            );

            // Seed Document Types
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM documents.Document_Type WHERE Code='VISIT_NOTE')
INSERT INTO documents.Document_Type (Code,Name,Description) VALUES ('VISIT_NOTE','Visit Note','Clinical visit document');
IF NOT EXISTS (SELECT 1 FROM documents.Document_Type WHERE Code='PRESCRIPTION')
INSERT INTO documents.Document_Type (Code,Name,Description) VALUES ('PRESCRIPTION','Prescription','Medication order');
IF NOT EXISTS (SELECT 1 FROM documents.Document_Type WHERE Code='REFERRAL')
INSERT INTO documents.Document_Type (Code,Name,Description) VALUES ('REFERRAL','Referral','Referral to specialist/provider');
IF NOT EXISTS (SELECT 1 FROM documents.Document_Type WHERE Code='SICK_LEAVE')
INSERT INTO documents.Document_Type (Code,Name,Description) VALUES ('SICK_LEAVE','Sick Leave','Work absence certificate');
IF NOT EXISTS (SELECT 1 FROM documents.Document_Type WHERE Code='LAB_RESULTS')
INSERT INTO documents.Document_Type (Code,Name,Description) VALUES ('LAB_RESULTS','Lab Results','Laboratory results report');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Lab_Test_Result", schema: "documents");
            migrationBuilder.DropTable(name: "Documents_Assigned", schema: "documents");
            migrationBuilder.DropTable(name: "Lab_Results", schema: "documents");
            migrationBuilder.DropTable(name: "Sick_Leave", schema: "documents");
            migrationBuilder.DropTable(name: "Referral", schema: "documents");
            migrationBuilder.DropTable(name: "Prescription", schema: "documents");
            migrationBuilder.DropTable(name: "Visit_Document", schema: "documents");
            migrationBuilder.DropTable(name: "Document", schema: "documents");
            migrationBuilder.DropTable(name: "Document_Type", schema: "documents");
        }
    }
}
