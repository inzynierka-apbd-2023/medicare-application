-- =========================================================================================
-- SQL Script to Populate Medicare Application Users
-- =========================================================================================
-- INSTRUCTIONS:
-- 1. Go to the Azure Portal -> SQL Database (UserServiceDb) -> Query Editor.
-- 2. Login with your admin credentials.
-- 3. Run this script.
--
-- IMPORTANT: You MUST replace the 'HASH_FOR_...' placeholders below with valid BCrypt hashes.
-- You can generate these at: https://bcrypt-generator.com/ (Use 10-12 rounds)
-- For example, for 'P@ssw0rd!', generate the hash and paste it in.
-- =========================================================================================

-- Ensure we are in the right context (Azure SQL doesn't support USE, but ensures schema usage)

-- 1. Get Role IDs
DECLARE @PatientRole NVARCHAR(450) = (SELECT Id FROM [user].[Role] WHERE Name = 'Patient');
DECLARE @DoctorRole NVARCHAR(450) = (SELECT Id FROM [user].[Role] WHERE Name = 'Doctor');
DECLARE @AdminRole NVARCHAR(450) = (SELECT Id FROM [user].[Role] WHERE Name = 'Admin');

IF @PatientRole IS NULL OR @DoctorRole IS NULL OR @AdminRole IS NULL
BEGIN
    PRINT 'ERROR: Roles not found. Ensure the application has started at least once to seed roles.';
    RETURN;
END

-- 2. Insert Users

-- User 1: Patient A
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'patient_a_20250818')
BEGIN
    DECLARE @Uid1 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid1, @PatientRole, 'patient_a_20250818', 'HASH_FOR_P@ssw0rd!', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid1, 'Patient', 'A', 'patient_a_20250818@test.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: patient_a_20250818';
END

-- User 2: Doctor A
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'doctor_a_20250818')
BEGIN
    DECLARE @Uid2 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid2, @DoctorRole, 'doctor_a_20250818', 'HASH_FOR_P@ssw0rd!', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid2, 'Doctor', 'A', 'doctor_a_20250818@test.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: doctor_a_20250818';
END

-- User 3: Reception A (Mapped to Admin as Receptionist role does not exist)
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'reception_a_20250818')
BEGIN
    DECLARE @Uid3 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid3, @AdminRole, 'reception_a_20250818', 'HASH_FOR_P@ssw0rd!', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid3, 'Reception', 'A', 'reception_a_20250818@test.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: reception_a_20250818';
END

-- User 4: Admin A
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'admin_a_20250818')
BEGIN
    DECLARE @Uid4 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid4, @AdminRole, 'admin_a_20250818', 'HASH_FOR_P@ssw0rd!', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid4, 'Admin', 'A', 'admin_a_20250818@test.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: admin_a_20250818';
END

-- User 5: Owner (Jerzy) (Mapped to Admin)
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'jancewiczjerzy2')
BEGIN
    DECLARE @Uid5 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    -- Password: Niewodnica5!
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid5, @AdminRole, 'jancewiczjerzy2', 'HASH_FOR_Niewodnica5!', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid5, 'Jerzy', 'Jancewicz', 'jancewiczjerzy2@gmail.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: jancewiczjerzy2';
END

-- User 6: Doctor S (Password: s}d$q1LhX3fM)
IF NOT EXISTS (SELECT 1 FROM [user].[User] WHERE Username = 'doctor_s_20250820_4')
BEGIN
    DECLARE @Uid6 NVARCHAR(450) = CONVERT(NVARCHAR(36), NEWID());
    INSERT INTO [user].[User] (Id, Role_Id, Username, PasswordHash, Is_Active, Created_At, Updated_At)
    VALUES (@Uid6, @DoctorRole, 'doctor_s_20250820_4', 'HASH_FOR_s}d$q1LhX3fM', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO [user].[User_Profile] (User_Id, FirstName, LastName, Email, Created_At, Updated_At)
    VALUES (@Uid6, 'Doctor', 'S', 'doctor_s_20250820_4@test.com', SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Created user: doctor_s_20250820_4';
END
