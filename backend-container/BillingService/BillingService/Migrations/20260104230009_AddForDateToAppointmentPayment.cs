using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Migrations
{
    /// <inheritdoc />
    public partial class AddForDateToAppointmentPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ForDate",
                schema: "billing",
                table: "Appointment_Payment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // View Creation moved from Program.cs
            migrationBuilder.Sql("IF OBJECT_ID('billing.vw_Patient_Billing_Summary', 'U') IS NOT NULL DROP TABLE billing.vw_Patient_Billing_Summary");
            migrationBuilder.Sql("DROP VIEW IF EXISTS billing.vw_Patient_Billing_Summary");
            migrationBuilder.Sql(@"
                CREATE VIEW billing.vw_Patient_Billing_Summary AS
                SELECT 
                    pm.PatientId,
                    COUNT(DISTINCT pi.Id) AS TotalPaymentIntents,
                    SUM(CASE WHEN pi.Status = 3 THEN pi.AmountCents ELSE 0 END) AS TotalPaidAmount,
                    SUM(CASE WHEN pi.Status = 0 THEN pi.AmountCents ELSE 0 END) AS TotalPendingAmount,
                    MAX(pi.CreatedAt) AS LastPaymentDate
                FROM billing.Payment_Method pm
                LEFT JOIN billing.Payment_Intent pi ON pi.PatientId = pm.PatientId
                GROUP BY pm.PatientId;
            ");

            migrationBuilder.Sql("IF OBJECT_ID('billing.vw_Doctor_Revenue_Dashboard', 'U') IS NOT NULL DROP TABLE billing.vw_Doctor_Revenue_Dashboard");
            migrationBuilder.Sql("DROP VIEW IF EXISTS billing.vw_Doctor_Revenue_Dashboard");
            migrationBuilder.Sql(@"
                CREATE VIEW billing.vw_Doctor_Revenue_Dashboard AS
                SELECT 
                    CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS DoctorId,
                    COUNT(ap.Id) AS TotalAppointmentPayments,
                    SUM(ap.AmountCents) AS TotalRevenue,
                    AVG(ap.AmountCents) AS AveragePaymentAmount
                FROM billing.Appointment_Payment ap
                GROUP BY PatientId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS billing.vw_Patient_Billing_Summary");
            migrationBuilder.Sql("DROP VIEW IF EXISTS billing.vw_Doctor_Revenue_Dashboard");

            migrationBuilder.DropColumn(
                name: "ForDate",
                schema: "billing",
                table: "Appointment_Payment");
        }
    }
}
