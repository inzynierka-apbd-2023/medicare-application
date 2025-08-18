using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

#nullable disable

namespace PractitionerService.Migrations
{
    [Migration("20250818190000_ResetPractitionerCleanSeed")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class ResetPractitionerCleanSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Purge all practitioner data and reseed a clean catalog with 1:1 Doctor-Specialization
            migrationBuilder.Sql(@"
-- Purge practitioner schema (safe order)
IF OBJECT_ID('practitioner.Doctor_Schedule','U') IS NOT NULL DELETE FROM practitioner.Doctor_Schedule;
IF OBJECT_ID('practitioner.Doctor_Specialization','U') IS NOT NULL DELETE FROM practitioner.Doctor_Specialization;
IF OBJECT_ID('practitioner.Specialization_Service','U') IS NOT NULL DELETE FROM practitioner.Specialization_Service;
IF OBJECT_ID('practitioner.Receptionist','U') IS NOT NULL DELETE FROM practitioner.Receptionist;
IF OBJECT_ID('practitioner.Doctor','U') IS NOT NULL DELETE FROM practitioner.Doctor;
IF OBJECT_ID('practitioner.Service','U') IS NOT NULL DELETE FROM practitioner.Service;
IF OBJECT_ID('practitioner.Specialization','U') IS NOT NULL DELETE FROM practitioner.Specialization;

-- Seed Specializations
INSERT INTO practitioner.Specialization (Name) VALUES
    ('General Practitioner'),
    ('Cardiologist'),
    ('Dermatologist'),
    ('Pediatrician'),
    ('Orthopedist');

-- Seed Services (include multi-specialization ones like Follow-up, Telehealth)
INSERT INTO practitioner.Service (Name, Description) VALUES
    ('General Consultation','Routine check and consultation'),
    ('Cardiology Review','Heart health assessment'),
    ('Dermatology Check','Skin examination and assessment'),
    ('Pediatric Visit','Child health appointment'),
    ('Orthopedic Assessment','Bone and joint evaluation'),
    ('Follow-up','Short follow-up visit after initial appointment'),
    ('Telehealth','Remote video consultation'),
    ('Lab Results Review','Discussion of lab results');

-- Map Services to Specializations (multi specialization for Follow-up, Telehealth, Lab Results Review)
DECLARE @gpSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='General Practitioner');
DECLARE @cardSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Cardiologist');
DECLARE @dermSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Dermatologist');
DECLARE @pedSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Pediatrician');
DECLARE @orthoSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Orthopedist');

DECLARE @svcGeneral nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='General Consultation');
DECLARE @svcCard nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Cardiology Review');
DECLARE @svcDerm nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Dermatology Check');
DECLARE @svcPed nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Pediatric Visit');
DECLARE @svcOrtho nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Orthopedic Assessment');
DECLARE @svcFollow nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Follow-up');
DECLARE @svcTele nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Telehealth');
DECLARE @svcLab nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Lab Results Review');

-- One-to-one primary services
IF @gpSpec IS NOT NULL AND @svcGeneral IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@gpSpec, @svcGeneral);
IF @cardSpec IS NOT NULL AND @svcCard IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@cardSpec, @svcCard);
IF @dermSpec IS NOT NULL AND @svcDerm IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@dermSpec, @svcDerm);
IF @pedSpec IS NOT NULL AND @svcPed IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@pedSpec, @svcPed);
IF @orthoSpec IS NOT NULL AND @svcOrtho IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@orthoSpec, @svcOrtho);

-- Multi-specialization services
DECLARE @specs TABLE (Id nvarchar(36));
INSERT INTO @specs SELECT Id FROM practitioner.Specialization;
DECLARE @sid nvarchar(36);
DECLARE spec_cursor CURSOR FOR SELECT Id FROM @specs;
OPEN spec_cursor;
FETCH NEXT FROM spec_cursor INTO @sid;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @svcFollow IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@sid, @svcFollow);
    IF @svcTele IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@sid, @svcTele);
    IF @svcLab IS NOT NULL INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@sid, @svcLab);
    FETCH NEXT FROM spec_cursor INTO @sid;
END
CLOSE spec_cursor; DEALLOCATE spec_cursor;

-- Create one doctor per specialization, with corresponding user and profile
-- We'll create minimal user records with Username (email-like) and a profile; PasswordHash left NULL for now
DECLARE @now datetime2 = SYSUTCDATETIME();

-- Helper to create a user and profile and return the new user id
-- Using inline logic per specialization for clarity

DECLARE @gpUser nvarchar(36) = CONVERT(varchar(36), NEWID());
IF @gpSpec IS NOT NULL
BEGIN
    INSERT INTO [user].[User] (Id, Role_Id, Schedule_Id, Created_At, Updated_At, Is_Active, Username, PasswordHash)
    VALUES (@gpUser, NULL, NULL, @now, @now, 1, 'doctor.gp', NULL);
    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Phone, DateOfBirth, Gender, Avatar_Url, Address_Line1, Address_Line2, City, State, ZipCode, Country, Created_At, Updated_At)
    VALUES (@gpUser, 'John', 'General', 'doctor.gp@example.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @now, @now);
END

DECLARE @cardUser nvarchar(36) = CONVERT(varchar(36), NEWID());
IF @cardSpec IS NOT NULL
BEGIN
    INSERT INTO [user].[User] (Id, Role_Id, Schedule_Id, Created_At, Updated_At, Is_Active, Username, PasswordHash)
    VALUES (@cardUser, NULL, NULL, @now, @now, 1, 'doctor.cardio', NULL);
    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Phone, DateOfBirth, Gender, Avatar_Url, Address_Line1, Address_Line2, City, State, ZipCode, Country, Created_At, Updated_At)
    VALUES (@cardUser, 'Carla', 'Cardio', 'doctor.cardio@example.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @now, @now);
END

DECLARE @dermUser nvarchar(36) = CONVERT(varchar(36), NEWID());
IF @dermSpec IS NOT NULL
BEGIN
    INSERT INTO [user].[User] (Id, Role_Id, Schedule_Id, Created_At, Updated_At, Is_Active, Username, PasswordHash)
    VALUES (@dermUser, NULL, NULL, @now, @now, 1, 'doctor.derm', NULL);
    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Phone, DateOfBirth, Gender, Avatar_Url, Address_Line1, Address_Line2, City, State, ZipCode, Country, Created_At, Updated_At)
    VALUES (@dermUser, 'Derek', 'Derm', 'doctor.derm@example.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @now, @now);
END

DECLARE @pedUser nvarchar(36) = CONVERT(varchar(36), NEWID());
IF @pedSpec IS NOT NULL
BEGIN
    INSERT INTO [user].[User] (Id, Role_Id, Schedule_Id, Created_At, Updated_At, Is_Active, Username, PasswordHash)
    VALUES (@pedUser, NULL, NULL, @now, @now, 1, 'doctor.peds', NULL);
    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Phone, DateOfBirth, Gender, Avatar_Url, Address_Line1, Address_Line2, City, State, ZipCode, Country, Created_At, Updated_At)
    VALUES (@pedUser, 'Pam', 'Peds', 'doctor.peds@example.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @now, @now);
END

DECLARE @orthoUser nvarchar(36) = CONVERT(varchar(36), NEWID());
IF @orthoSpec IS NOT NULL
BEGIN
    INSERT INTO [user].[User] (Id, Role_Id, Schedule_Id, Created_At, Updated_At, Is_Active, Username, PasswordHash)
    VALUES (@orthoUser, NULL, NULL, @now, @now, 1, 'doctor.ortho', NULL);
    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Phone, DateOfBirth, Gender, Avatar_Url, Address_Line1, Address_Line2, City, State, ZipCode, Country, Created_At, Updated_At)
    VALUES (@orthoUser, 'Owen', 'Ortho', 'doctor.ortho@example.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @now, @now);
END

-- Insert Doctors (1:1 with specialization) and link specialization
DECLARE @dGp nvarchar(36) = NULL, @dCard nvarchar(36) = NULL, @dDerm nvarchar(36) = NULL, @dPed nvarchar(36) = NULL, @dOrtho nvarchar(36) = NULL;
IF @gpSpec IS NOT NULL BEGIN INSERT INTO practitioner.Doctor (UserId, Bio) VALUES (@gpUser, 'General practice physician'); SET @dGp = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE UserId=@gpUser); INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@dGp, @gpSpec); END
IF @cardSpec IS NOT NULL BEGIN INSERT INTO practitioner.Doctor (UserId, Bio) VALUES (@cardUser, 'Cardiology specialist'); SET @dCard = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE UserId=@cardUser); INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@dCard, @cardSpec); END
IF @dermSpec IS NOT NULL BEGIN INSERT INTO practitioner.Doctor (UserId, Bio) VALUES (@dermUser, 'Dermatology specialist'); SET @dDerm = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE UserId=@dermUser); INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@dDerm, @dermSpec); END
IF @pedSpec IS NOT NULL BEGIN INSERT INTO practitioner.Doctor (UserId, Bio) VALUES (@pedUser, 'Pediatrics specialist'); SET @dPed = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE UserId=@pedUser); INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@dPed, @pedSpec); END
IF @orthoSpec IS NOT NULL BEGIN INSERT INTO practitioner.Doctor (UserId, Bio) VALUES (@orthoUser, 'Orthopedics specialist'); SET @dOrtho = (SELECT TOP 1 Id FROM practitioner.Doctor WHERE UserId=@orthoUser); INSERT INTO practitioner.Doctor_Specialization (DoctorId, SpecializationId) VALUES (@dOrtho, @orthoSpec); END

-- Schedules for each doctor (simple weekly slots)
IF @dGp IS NOT NULL BEGIN INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES (@dGp, 1, '09:00','12:00'), (@dGp, 3, '13:00','17:00'); END
IF @dCard IS NOT NULL BEGIN INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES (@dCard, 2, '10:00','14:00'); END
IF @dDerm IS NOT NULL BEGIN INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES (@dDerm, 4, '09:00','12:00'); END
IF @dPed IS NOT NULL BEGIN INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES (@dPed, 5, '10:00','13:00'); END
IF @dOrtho IS NOT NULL BEGIN INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES (@dOrtho, 1, '14:00','17:00'); END

-- Refresh view
IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort rollback: remove doctors and users created with known usernames; remove seeded catalog entries
            migrationBuilder.Sql(@"
-- Remove practitioner seeded data
DELETE ds FROM practitioner.Doctor_Schedule ds WHERE ds.DoctorId IN (SELECT Id FROM practitioner.Doctor);
DELETE ds FROM practitioner.Doctor_Specialization ds WHERE ds.DoctorId IN (SELECT Id FROM practitioner.Doctor);
DELETE FROM practitioner.Doctor;
DELETE FROM practitioner.Specialization_Service;
DELETE FROM practitioner.Service WHERE Name IN ('General Consultation','Cardiology Review','Dermatology Check','Pediatric Visit','Orthopedic Assessment','Follow-up','Telehealth','Lab Results Review');
DELETE FROM practitioner.Specialization WHERE Name IN ('General Practitioner','Cardiologist','Dermatologist','Pediatrician','Orthopedist');

-- Remove created user accounts by usernames we inserted
DELETE up FROM [user].[User_Profile] up WHERE up.User_Id IN (SELECT u.Id FROM [user].[User] u WHERE u.Username IN ('doctor.gp','doctor.cardio','doctor.derm','doctor.peds','doctor.ortho'));
DELETE FROM [user].[User] WHERE Username IN ('doctor.gp','doctor.cardio','doctor.derm','doctor.peds','doctor.ortho');

IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';

");
        }
    }
}
