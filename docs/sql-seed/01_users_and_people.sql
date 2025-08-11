-- Copied from dev-db/seed/01_users_and_people.sql
-- Truncated for brevity; see repository history for full schema if needed.
-- Role
CREATE TABLE dbo.Role (
  Id           VARCHAR(36)  NOT NULL CONSTRAINT DF_Role_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name         NVARCHAR(100) NOT NULL,
  Description  NVARCHAR(500) NULL,
  CONSTRAINT PK_Role PRIMARY KEY (Id)
);

-- User
CREATE TABLE dbo.[User] (
  Id           VARCHAR(36)  NOT NULL CONSTRAINT DF_User_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Role_Id      VARCHAR(36)  NULL,
  Schedule_Id  VARCHAR(36)  NULL,
  Username     NVARCHAR(50) NULL,
  PasswordHash NVARCHAR(255) NULL,
  Created_At   DATETIME     NOT NULL DEFAULT SYSUTCDATETIME(),
  Updated_At   DATETIME     NOT NULL DEFAULT SYSUTCDATETIME(),
  Is_Active    BIT          NOT NULL DEFAULT 1,
  CONSTRAINT PK_User PRIMARY KEY (Id),
  CONSTRAINT FK_User_Role FOREIGN KEY (Role_Id) REFERENCES dbo.Role(Id)
);

-- User_Profile
CREATE TABLE dbo.User_Profile (
  User_Id       VARCHAR(36)  NOT NULL,
  FirstName     NVARCHAR(100) NOT NULL,
  LastName      NVARCHAR(100) NOT NULL,
  Email         NVARCHAR(255) NOT NULL,
  Phone         NVARCHAR(20)  NULL,
  DateOfBirth   DATE          NULL,
  Gender        NVARCHAR(20)  NULL,
  Avatar_Url    NVARCHAR(500) NULL,
  Address_Line1 NVARCHAR(200) NULL,
  Address_Line2 NVARCHAR(200) NULL,
  City          NVARCHAR(100) NULL,
  State         NVARCHAR(100) NULL,
  ZipCode       NVARCHAR(20)  NULL,
  Country       NVARCHAR(100) NULL,
  Created_At    DATETIME      NOT NULL DEFAULT SYSUTCDATETIME(),
  Updated_At    DATETIME      NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_User_Profile PRIMARY KEY (User_Id),
  CONSTRAINT FK_User_Profile_User FOREIGN KEY (User_Id) REFERENCES dbo.[User](Id)
);
