-- Migration script to add authentication fields to existing User table
-- This script adds Username and PasswordHash columns to support authentication

USE medicare_dev;
GO

-- Add Username column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'User' AND COLUMN_NAME = 'Username')
BEGIN
    ALTER TABLE dbo.[User] ADD Username NVARCHAR(50) NULL;
    PRINT 'Added Username column to User table';
END
ELSE
BEGIN
    PRINT 'Username column already exists in User table';
END

-- Add PasswordHash column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'User' AND COLUMN_NAME = 'PasswordHash')
BEGIN
    ALTER TABLE dbo.[User] ADD PasswordHash NVARCHAR(255) NULL;
    PRINT 'Added PasswordHash column to User table';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists in User table';
END

-- Add unique constraint on Username if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Username' AND object_id = OBJECT_ID('dbo.[User]'))
BEGIN
    -- Note: We'll add the unique constraint after we have some data
    PRINT 'Unique index on Username will be added after data setup';
END
ELSE
BEGIN
    PRINT 'Unique index on Username already exists';
END

-- Add some default roles if they don't exist
IF NOT EXISTS (SELECT * FROM dbo.Role WHERE Name = 'Admin')
BEGIN
    INSERT INTO dbo.Role (Name, Description) VALUES ('Admin', 'System Administrator');
    PRINT 'Added Admin role';
END

IF NOT EXISTS (SELECT * FROM dbo.Role WHERE Name = 'Doctor')
BEGIN
    INSERT INTO dbo.Role (Name, Description) VALUES ('Doctor', 'Medical Doctor');
    PRINT 'Added Doctor role';
END

IF NOT EXISTS (SELECT * FROM dbo.Role WHERE Name = 'Patient')
BEGIN
    INSERT INTO dbo.Role (Name, Description) VALUES ('Patient', 'Patient');
    PRINT 'Added Patient role';
END

PRINT 'Migration completed successfully';
