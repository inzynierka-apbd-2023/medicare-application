using System;
using BillingService.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BillingService.Migrations
{
    [DbContext(typeof(BillingDbContext))]
    [Migration("20250814160000_InitBilling")]
    public partial class InitBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "billing");

            migrationBuilder.CreateTable(
                name: "Payment_Method",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ProviderToken = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Last4 = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    Brand = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table => { table.PrimaryKey("PK_Payment_Method", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Method_Patient_IsDefault",
                schema: "billing",
                table: "Payment_Method",
                columns: new[] { "PatientId", "IsDefault" });

            migrationBuilder.CreateTable(
                name: "Subscription_Contract",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PatientId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    PlanCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DefaultPaymentMethodId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Subscription_Contract", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Payment_Intent",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    PatientId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ProviderIntentId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    AmountCents = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table => { table.PrimaryKey("PK_Payment_Intent", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Intent_Patient_Kind_Status",
                schema: "billing",
                table: "Payment_Intent",
                columns: new[] { "PatientId", "Kind", "Status" });

            migrationBuilder.CreateTable(
                name: "Payment_Transaction",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    PaymentIntentId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AmountCents = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ProviderChargeId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ProviderRefundId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    FailureCode = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    FailureMessage = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Payment_Transaction", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Transaction_PaymentIntentId",
                schema: "billing",
                table: "Payment_Transaction",
                column: "PaymentIntentId");

            migrationBuilder.CreateTable(
                name: "Appointment_Payment",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    AppointmentId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    PatientId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    AmountCents = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    PaymentIntentId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Appointment_Payment", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Payment_AppointmentId",
                schema: "billing",
                table: "Appointment_Payment",
                column: "AppointmentId");

            migrationBuilder.CreateTable(
                name: "Subscription_Payment",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    SubscriptionContractId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    PatientId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false),
                    PlanCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountCents = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    PaymentIntentId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Subscription_Payment", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_Payment_Contract_Period",
                schema: "billing",
                table: "Subscription_Payment",
                columns: new[] { "SubscriptionContractId", "PeriodStart" });

            migrationBuilder.CreateTable(
                name: "Psp_Webhook_Event",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Psp_Webhook_Event", x => new { x.Id, x.Provider }); });

            migrationBuilder.CreateTable(
                name: "Outbox_Event",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(VARCHAR(36), NEWID())"),
                    Type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Outbox_Event", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Event_PublishedAt",
                schema: "billing",
                table: "Outbox_Event",
                column: "PublishedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Outbox_Event", schema: "billing");
            migrationBuilder.DropTable(name: "Psp_Webhook_Event", schema: "billing");
            migrationBuilder.DropTable(name: "Subscription_Payment", schema: "billing");
            migrationBuilder.DropTable(name: "Appointment_Payment", schema: "billing");
            migrationBuilder.DropTable(name: "Payment_Transaction", schema: "billing");
            migrationBuilder.DropTable(name: "Payment_Intent", schema: "billing");
            migrationBuilder.DropTable(name: "Subscription_Contract", schema: "billing");
            migrationBuilder.DropTable(name: "Payment_Method", schema: "billing");
        }
    }
}
