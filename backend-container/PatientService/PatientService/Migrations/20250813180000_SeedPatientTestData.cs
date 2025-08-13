using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PatientService.Data;

#nullable disable

namespace PatientService.Migrations
{
    [Migration("20250813180000_SeedPatientTestData")]
    [DbContext(typeof(PatientDbContext))]
    public partial class SeedPatientTestData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fixed IDs for deterministic test data
            var pA = "11111111-1111-1111-1111-111111111111";
            var pB = "22222222-2222-2222-2222-222222222222";
            var uA = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // sample user id A
            var uB = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // sample user id B
            var doc1 = "dddddddd-dddd-dddd-dddd-dddddddddddd"; // sample doctor id

            // Patients
            migrationBuilder.Sql($@"
INSERT INTO patient.Patient (Id, UserId, PrimaryDoctorId)
VALUES ('{pA}', '{uA}', '{doc1}');
INSERT INTO patient.Patient (Id, UserId, PrimaryDoctorId)
VALUES ('{pB}', '{uB}', NULL);

-- Initial statuses
INSERT INTO patient.Patient_Status (Id, PatientId, Status)
VALUES ('31111111-1111-1111-1111-111111111111', '{pA}', N'Active');
INSERT INTO patient.Patient_Status (Id, PatientId, Status)
VALUES ('32222222-2222-2222-2222-222222222222', '{pB}', N'Active');

-- Emergency contacts
INSERT INTO patient.Emergency_Contact (Id, PatientId, Name, Relation, Phone)
VALUES ('41111111-1111-1111-1111-111111111111', '{pA}', N'Jane Example', N'Spouse', N'+1-555-1000');
INSERT INTO patient.Emergency_Contact (Id, PatientId, Name, Relation, Phone)
VALUES ('42222222-2222-2222-2222-222222222222', '{pB}', N'John Sample', N'Sibling', N'+1-555-2000');

-- Insurance
INSERT INTO patient.Insurance (Id, PatientId, Provider, PolicyNumber, ValidFrom, ValidTo)
VALUES ('51111111-1111-1111-1111-111111111111', '{pA}', N'Aetna', N'AET-001', '2025-01-01T00:00:00Z', '2026-01-01T00:00:00Z');
INSERT INTO patient.Insurance (Id, PatientId, Provider, PolicyNumber, ValidFrom, ValidTo)
VALUES ('52222222-2222-2222-2222-222222222222', '{pB}', N'BlueCross', N'BC-002', '2025-02-01T00:00:00Z', '2026-02-01T00:00:00Z');

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete in FK-friendly order
            migrationBuilder.Sql(@"
DELETE FROM patient.Insurance WHERE Id IN ('51111111-1111-1111-1111-111111111111','52222222-2222-2222-2222-222222222222');
DELETE FROM patient.Emergency_Contact WHERE Id IN ('41111111-1111-1111-1111-111111111111','42222222-2222-2222-2222-222222222222');
DELETE FROM patient.Patient_Status WHERE Id IN ('31111111-1111-1111-1111-111111111111','32222222-2222-2222-2222-222222222222');
DELETE FROM patient.Patient WHERE Id IN ('11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222');
");
        }
    }
}
