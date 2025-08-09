-- ROLE
CREATE TABLE dbo.Role (
  Id           VARCHAR(36)  NOT NULL CONSTRAINT DF_Role_Id DEFAULT dbo.NewGuidString(),
  Name         NVARCHAR(100) NOT NULL,
  Description  NVARCHAR(500) NULL,
  CONSTRAINT PK_Role PRIMARY KEY (Id)
);

-- USER (system account)
CREATE TABLE dbo.[User] (
  Id           VARCHAR(36)  NOT NULL CONSTRAINT DF_User_Id DEFAULT dbo.NewGuidString(),
  Role_Id      VARCHAR(36)  NULL,
  Schedule_Id  VARCHAR(36)  NULL, -- FK later (Schedule)
  Created_At   DATETIME     NOT NULL DEFAULT SYSUTCDATETIME(),
  Updated_At   DATETIME     NOT NULL DEFAULT SYSUTCDATETIME(),
  Is_Active    BIT          NOT NULL DEFAULT 1,
  CONSTRAINT PK_User PRIMARY KEY (Id),
  CONSTRAINT FK_User_Role FOREIGN KEY (Role_Id) REFERENCES dbo.Role(Id)
);

-- USER PROFILE
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

-- DOCTOR (1-1 with User in your model)
CREATE TABLE dbo.Doctor (
  Id              VARCHAR(36)  NOT NULL, -- matches User.Id
  License_Num     NVARCHAR(100) NULL,
  Years_Experi    INT          NULL,
  Biography       NVARCHAR(2000) NULL,
  Office_Addre    NVARCHAR(500) NULL,
  CONSTRAINT PK_Doctor PRIMARY KEY (Id),
  CONSTRAINT FK_Doctor_User FOREIGN KEY (Id) REFERENCES dbo.[User](Id)
);

-- RECEPTIONIST (1-1 with User)
CREATE TABLE dbo.Receptionist (
  Id         VARCHAR(36) NOT NULL, -- matches User.Id
  Departm    NVARCHAR(200) NULL,
  CONSTRAINT PK_Receptionist PRIMARY KEY (Id),
  CONSTRAINT FK_Receptionist_User FOREIGN KEY (Id) REFERENCES dbo.[User](Id)
);

-- PATIENT (separate entity with optional link to a Doctor)
CREATE TABLE dbo.Patient (
  Id                    VARCHAR(36)  NOT NULL CONSTRAINT DF_Patient_Id DEFAULT dbo.NewGuidString(),
  General_Doctor_Id     VARCHAR(36)  NULL, -- User/Doctor
  Medical_Record_Numbe  NVARCHAR(100) NULL,
  Blood_Type            NVARCHAR(10) NULL,
  Height_cm             DECIMAL(5,2) NULL,
  Weight_kg             DECIMAL(5,2) NULL,
  CONSTRAINT PK_Patient PRIMARY KEY (Id),
  CONSTRAINT FK_Patient_GeneralDoctor FOREIGN KEY (General_Doctor_Id) REFERENCES dbo.Doctor(Id)
);
CREATE INDEX IX_Patient_GeneralDoctor ON dbo.Patient(General_Doctor_Id);

-- EMERGENCY CONTACT
CREATE TABLE dbo.Emergency_Contact (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_EmergencyContact_Id DEFAULT dbo.NewGuidString(),
  Patient_Id  VARCHAR(36) NOT NULL,
  Name        NVARCHAR(200) NOT NULL,
  Phone       NVARCHAR(20)  NULL,
  Relationship NVARCHAR(100) NULL,
  Is_Primary  BIT NOT NULL DEFAULT 0,
  Created_At  DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Emergency_Contact PRIMARY KEY (Id),
  CONSTRAINT FK_Emergency_Contact_Patient FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id)
);

-- INSURANCE
CREATE TABLE dbo.Insurance (
  Patient_Id     VARCHAR(36)  NOT NULL,
  Provider_Name  NVARCHAR(200) NOT NULL,
  Policy_Number  NVARCHAR(100) NOT NULL,
  Group_Number   NVARCHAR(100) NULL,
  Valid_From     DATE NOT NULL,
  Valid_To       DATE NULL,
  Is_Primary     BIT  NOT NULL DEFAULT 1,
  Is_Active      BIT  NOT NULL DEFAULT 1,
  CONSTRAINT PK_Insurance PRIMARY KEY (Patient_Id, Provider_Name, Policy_Number),
  CONSTRAINT FK_Insurance_Patient FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id)
);
