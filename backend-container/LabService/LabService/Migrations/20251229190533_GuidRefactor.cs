using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabService.Migrations
{
    /// <inheritdoc />
    public partial class GuidRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lab");

            migrationBuilder.CreateTable(
                name: "Lab_Order",
                schema: "lab",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderingDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Ordered"),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Normal"),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lab_Result",
                schema: "lab",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Flag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResultDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Result", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lab_Result_Review",
                schema: "lab",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LabResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Result_Review", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lab_Test",
                schema: "lab",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LabOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoincCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab_Test", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Order_OrderedDate",
                schema: "lab",
                table: "Lab_Order",
                column: "OrderedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Order_OrderingDoctorId",
                schema: "lab",
                table: "Lab_Order",
                column: "OrderingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Order_PatientId",
                schema: "lab",
                table: "Lab_Order",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Result_LabTestId",
                schema: "lab",
                table: "Lab_Result",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Result_PatientId",
                schema: "lab",
                table: "Lab_Result",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Result_ResultDate",
                schema: "lab",
                table: "Lab_Result",
                column: "ResultDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Result_Review_LabResultId",
                schema: "lab",
                table: "Lab_Result_Review",
                column: "LabResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Result_Review_ReviewedByDoctorId",
                schema: "lab",
                table: "Lab_Result_Review",
                column: "ReviewedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_LabOrderId",
                schema: "lab",
                table: "Lab_Test",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_Test_LoincCode",
                schema: "lab",
                table: "Lab_Test",
                column: "LoincCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lab_Order",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "Lab_Result",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "Lab_Result_Review",
                schema: "lab");

            migrationBuilder.DropTable(
                name: "Lab_Test",
                schema: "lab");
        }
    }
}
