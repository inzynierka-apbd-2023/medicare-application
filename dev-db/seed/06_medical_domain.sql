CREATE TABLE dbo.Service (
  Id           VARCHAR(36) NOT NULL CONSTRAINT DF_Service_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name         NVARCHAR(200) NOT NULL,
  Description  NVARCHAR(1000) NULL,
  Duration_Min INT NULL,
  Is_Active    BIT NOT NULL DEFAULT 1,
  Created_At   DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Service PRIMARY KEY (Id)
);

CREATE TABLE dbo.Specialization (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_Spec_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name        NVARCHAR(200) NOT NULL,
  Description NVARCHAR(1000) NULL,
  Service_Id  VARCHAR(36) NULL,
  Is_Active   BIT NOT NULL DEFAULT 1,
  CONSTRAINT PK_Specialization PRIMARY KEY (Id),
  CONSTRAINT FK_Spec_Service FOREIGN KEY (Service_Id) REFERENCES dbo.Service(Id)
);

CREATE TABLE dbo.Doctor_Specialization (
  Id                VARCHAR(36) NOT NULL CONSTRAINT DF_DocSpec_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Doctor_Id         VARCHAR(36) NOT NULL,
  Specialization_Id VARCHAR(36) NOT NULL,
  Is_Primary        BIT NOT NULL DEFAULT 0,
  Certified_Date    DATE NULL,
  CONSTRAINT PK_Doctor_Specialization PRIMARY KEY (Id),
  CONSTRAINT FK_DocSpec_Doctor FOREIGN KEY (Doctor_Id) REFERENCES dbo.Doctor(Id),
  CONSTRAINT FK_DocSpec_Spec FOREIGN KEY (Specialization_Id) REFERENCES dbo.Specialization(Id),
  CONSTRAINT UQ_DocSpec UNIQUE (Doctor_Id, Specialization_Id)
);

CREATE TABLE dbo.Medical_Condition (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_MedCond_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Code        NVARCHAR(50) NOT NULL,
  Name        NVARCHAR(200) NOT NULL,
  Description NVARCHAR(1000) NULL,
  Category    NVARCHAR(100) NULL,
  Is_Chronic  BIT NOT NULL DEFAULT 0,
  CONSTRAINT PK_Medical_Condition PRIMARY KEY (Id)
);

CREATE TABLE dbo.Patient_Medical_Condition (
  Id                   VARCHAR(36) NOT NULL CONSTRAINT DF_PMC_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Patient_Id           VARCHAR(36) NOT NULL,
  Medical_Condition_Id VARCHAR(36) NOT NULL,
  Diagnosed_Date       DATE NULL,
  Status               NVARCHAR(50) NULL,
  Severity             NVARCHAR(50) NULL,
  Notes                NVARCHAR(1000) NULL,
  Created_At           DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Updated_At           DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Patient_Medical_Condition PRIMARY KEY (Id),
  CONSTRAINT FK_PMC_Patient   FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id),
  CONSTRAINT FK_PMC_Condition FOREIGN KEY (Medical_Condition_Id) REFERENCES dbo.Medical_Condition(Id)
);

CREATE TABLE dbo.Rate (
  Id           VARCHAR(36) NOT NULL CONSTRAINT DF_Rate_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Rate_Value   TINYINT NOT NULL,
  Description  NVARCHAR(1000) NULL,
  Patient_User VARCHAR(36) NULL,
  Doctor_User  VARCHAR(36) NULL,
  Appointment  VARCHAR(36) NULL,
  Rated_At     DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Is_Anonymo   BIT NOT NULL DEFAULT 0,
  CONSTRAINT PK_Rate PRIMARY KEY (Id),
  CONSTRAINT FK_Rate_PatientUser FOREIGN KEY (Patient_User) REFERENCES dbo.[User](Id),
  CONSTRAINT FK_Rate_DoctorUser  FOREIGN KEY (Doctor_User)  REFERENCES dbo.[User](Id),
  CONSTRAINT FK_Rate_Appointment FOREIGN KEY (Appointment)  REFERENCES dbo.Appointment(Id)
);
