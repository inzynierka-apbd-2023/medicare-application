using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentsService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "documents");

            migrationBuilder.CreateTable(
                name: "Document_Type",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TemplatePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document_Type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lab_Test_Type",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LoincCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoincComponent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LoincProperty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LoincTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LoincSystem = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    LoincScale = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LoincMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExampleUnits = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Test_Type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents_Assigned",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Lab_Results",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Medication = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PharmacyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacyPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefillsRemaining = table.Column<int>(type: "int", nullable: true),
                    AtcCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AtcName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Referral",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Sick_Leave",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Visit_Document",
                schema: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Lab_Test_Result",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LabResultsDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoincCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ParameterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Lab_Test_Result_Lab_Test_Type_LabTestTypeId",
                        column: x => x.LabTestTypeId,
                        principalSchema: "documents",
                        principalTable: "Lab_Test_Type",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_DoctorId_CreatedAt",
                schema: "documents",
                table: "Document",
                columns: new[] { "DoctorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeId",
                schema: "documents",
                table: "Document",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_PatientId_Type_CreatedAt",
                schema: "documents",
                table: "Document",
                columns: new[] { "PatientId", "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Document_Type_Code",
                schema: "documents",
                table: "Document_Type",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Assigned_DocumentId_AppointmentId",
                schema: "documents",
                table: "Documents_Assigned",
                columns: new[] { "DocumentId", "AppointmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_Result_LabResultsDocumentId",
                schema: "documents",
                table: "Lab_Test_Result",
                column: "LabResultsDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_Result_LabTestTypeId",
                schema: "documents",
                table: "Lab_Test_Result",
                column: "LabTestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_Type_LoincCode",
                schema: "documents",
                table: "Lab_Test_Type",
                column: "LoincCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_AtcCode",
                schema: "documents",
                table: "Prescription",
                column: "AtcCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents_Assigned",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Lab_Test_Result",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Prescription",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Referral",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Sick_Leave",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Visit_Document",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Lab_Results",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Lab_Test_Type",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Document_Type",
                schema: "documents");
        }
    }
}
