using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

namespace PractitionerService.Migrations
{
    [Migration("20250813163000_SeedPractitionerTestData")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class SeedPractitionerTestData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Services (idempotent by name)
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='General Consultation')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('General Consultation','Routine check and consultation');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Cardiology Review')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Cardiology Review','Heart health assessment');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Dermatology Check')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Dermatology Check','Skin examination');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Pediatric Visit')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Pediatric Visit','Child health appointment');
IF NOT EXISTS (SELECT 1 FROM practitioner.Service WHERE Name='Orthopedic Assessment')
    INSERT INTO practitioner.Service (Name, Description) VALUES ('Orthopedic Assessment','Bone and joint evaluation');

-- Specializations
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='General Practitioner')
    INSERT INTO practitioner.Specialization (Name) VALUES ('General Practitioner');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Cardiologist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Cardiologist');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Dermatologist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Dermatologist');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Pediatrician')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Pediatrician');
IF NOT EXISTS (SELECT 1 FROM practitioner.Specialization WHERE Name='Orthopedist')
    INSERT INTO practitioner.Specialization (Name) VALUES ('Orthopedist');

-- Doctors (two sample doctors referencing existing user IDs if available)
IF NOT EXISTS (SELECT 1 FROM practitioner.Doctor)
BEGIN
    DECLARE @u1 nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] ORDER BY Created_At);
    IF @u1 IS NULL SET @u1 = CONVERT(varchar(36),NEWID());
    DECLARE @u2 nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] WHERE Id <> @u1 ORDER BY Created_At);
    IF @u2 IS NULL SET @u2 = CONVERT(varchar(36),NEWID());

    INSERT INTO practitioner.Doctor (UserId, Bio) VALUES
        (@u1, 'Experienced general practitioner with a focus on preventative care'),
        (@u2, 'Cardiology specialist with 10 years of clinical experience');

    -- Link doctor specializations
    DECLARE @gpId nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='General Practitioner');
    DECLARE @cardId nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Cardiologist');
    DECLARE @d1 nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Doctor ORDER BY CreatedAt);
    DECLARE @d2 nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE Id <> @d1 ORDER BY CreatedAt);
    IF @d1 IS NOT NULL AND @gpId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Specialization WHERE DoctorId=@d1 AND SpecializationId=@gpId)
        INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@d1, @gpId);
    IF @d2 IS NOT NULL AND @cardId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Specialization WHERE DoctorId=@d2 AND SpecializationId=@cardId)
        INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@d2, @cardId);

    -- Schedules
    IF @d1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Schedule WHERE DoctorId=@d1)
        INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES
            (@d1,1,'09:00','12:00'),
            (@d1,3,'13:00','17:00');
    IF @d2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Doctor_Schedule WHERE DoctorId=@d2)
        INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES
            (@d2,2,'10:00','14:00');
END

-- Receptionist (sample)
IF NOT EXISTS (SELECT 1 FROM practitioner.Receptionist)
BEGIN
    DECLARE @rUser nvarchar(36) = (SELECT TOP 1 Id FROM [user].[User] WHERE Id NOT IN (SELECT UserId FROM practitioner.Doctor) ORDER BY Created_At);
    IF @rUser IS NULL SET @rUser = CONVERT(varchar(36),NEWID());
    INSERT INTO practitioner.Receptionist (UserId) VALUES (@rUser);
END

-- Refresh view (if present)
IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove only seeded data (by names / generated bios)
            migrationBuilder.Sql(@"
DELETE FROM practitioner.Doctor_Schedule WHERE DoctorId IN (SELECT Id FROM practitioner.Doctor);
DELETE FROM practitioner.Doctor_Specialization WHERE DoctorId IN (SELECT Id FROM practitioner.Doctor);
DELETE FROM practitioner.Receptionist;
DELETE FROM practitioner.Doctor;
-- Keep services and specializations; comment out below if you want to retain catalog
DELETE FROM practitioner.Service WHERE Name IN ('General Consultation','Cardiology Review','Dermatology Check','Pediatric Visit','Orthopedic Assessment');
DELETE FROM practitioner.Specialization WHERE Name IN ('General Practitioner','Cardiologist','Dermatologist','Pediatrician','Orthopedist');
IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';
");
        }
    }
}
