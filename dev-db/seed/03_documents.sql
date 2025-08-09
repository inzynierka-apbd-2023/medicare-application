-- DOCUMENT TYPES
CREATE TABLE dbo.Document_Type (
  Id            VARCHAR(36) NOT NULL CONSTRAINT DF_DocType_Id DEFAULT dbo.NewGuidString(),
  Code          NVARCHAR(30)  NOT NULL,
  Name          NVARCHAR(100) NOT NULL,
  Description   NVARCHAR(255) NULL,
  Template_Path NVARCHAR(500) NULL,
  CONSTRAINT PK_Document_Type PRIMARY KEY (Id)
);

-- DOCUMENT
CREATE TABLE dbo.Document (
  Id                VARCHAR(36) NOT NULL CONSTRAINT DF_Document_Id DEFAULT dbo.NewGuidString(),
  Created_At        DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Notes             NVARCHAR(1000) NULL,
  Type              INT NULL, -- keep for compatibility if needed by app
  Document_Type     VARCHAR(36) NULL,
  Patient_Id        VARCHAR(36) NULL,
  Doctor_Id         VARCHAR(36) NULL,
  File_Path         NVARCHAR(1000) NULL,
  File_Size_Bytes   BIGINT NULL,
  CONSTRAINT PK_Document PRIMARY KEY (Id),
  CONSTRAINT FK_Document_Type FOREIGN KEY (Document_Type) REFERENCES dbo.Document_Type(Id),
  CONSTRAINT FK_Document_Patient FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id),
  CONSTRAINT FK_Document_Doctor FOREIGN KEY (Doctor_Id) REFERENCES dbo.Doctor(Id)
);

-- ASSIGN DOCUMENTS TO APPOINTMENTS
CREATE TABLE dbo.Documents_Assigned (
  Id             VARCHAR(36) NOT NULL CONSTRAINT DF_DocAssigned_Id DEFAULT dbo.NewGuidString(),
  Assigned_At    DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Appointment_Id VARCHAR(36) NOT NULL,
  Document_Id    VARCHAR(36) NOT NULL,
  CONSTRAINT PK_Documents_Assigned PRIMARY KEY (Id),
  CONSTRAINT UQ_Documents_Assigned UNIQUE (Appointment_Id, Document_Id),
  CONSTRAINT FK_DA_Appointment FOREIGN KEY (Appointment_Id) REFERENCES dbo.Appointment(Id),
  CONSTRAINT FK_DA_Document FOREIGN KEY (Document_Id) REFERENCES dbo.Document(Id)
);

-- VISIT DOCUMENT (1-1 with Document)
CREATE TABLE dbo.Visit_Document (
  Document_Id     VARCHAR(36) NOT NULL,
  Symptoms        NVARCHAR(MAX) NULL,
  Findings        NVARCHAR(MAX) NULL,
  Diagnosis       NVARCHAR(MAX) NULL,
  Recommendat     NVARCHAR(MAX) NULL,
  Vital_Signs     NVARCHAR(1000) NULL,
  Treatment_Pla   NVARCHAR(2000) NULL,
  Follow_Up_Dat   DATETIME NULL,
  CONSTRAINT PK_Visit_Document PRIMARY KEY (Document_Id),
  CONSTRAINT FK_Visit_Document_Document FOREIGN KEY (Document_Id) REFERENCES dbo.Document(Id)
);

-- LAB TEST TYPES & RESULTS
CREATE TABLE dbo.Lab_Test_Type (
  Id                VARCHAR(36) NOT NULL CONSTRAINT DF_LabTestType_Id DEFAULT dbo.NewGuidString(),
  Code              NVARCHAR(50)  NOT NULL,
  Name              NVARCHAR(200) NOT NULL,
  Description       NVARCHAR(500) NULL,
  Reference_Range   NVARCHAR(200) NULL,
  Unit              NVARCHAR(50)  NULL,
  Category          NVARCHAR(100) NULL,
  Normal_Min_Value  DECIMAL(18,6) NULL,
  Normal_Max_Value  DECIMAL(18,6) NULL,
  CONSTRAINT PK_Lab_Test_Type PRIMARY KEY (Id)
);

CREATE TABLE dbo.Lab_Results (
  Document_Id       VARCHAR(36) NOT NULL,
  Test_Type         NVARCHAR(200) NOT NULL,
  Test_Date         DATETIME NOT NULL,
  Laboratory        NVARCHAR(200) NULL,
  Overall_Status    NVARCHAR(50)  NULL,
  Interpretation    NVARCHAR(MAX) NULL,
  Reference_Rang    NVARCHAR(1000) NULL,
  Technician_Nam    NVARCHAR(200) NULL,
  Doctor_Commen     NVARCHAR(1000) NULL,
  CONSTRAINT PK_Lab_Results PRIMARY KEY (Document_Id),
  CONSTRAINT FK_Lab_Results_Document FOREIGN KEY (Document_Id) REFERENCES dbo.Document(Id)
);

CREATE TABLE dbo.Lab_Test_Result (
  Id                        VARCHAR(36) NOT NULL CONSTRAINT DF_LabTestResult_Id DEFAULT dbo.NewGuidString(),
  Lab_Results_Document_I    VARCHAR(36) NOT NULL,
  Lab_Test_Type_Id          VARCHAR(36) NOT NULL,
  Parameter_Name            NVARCHAR(200) NULL,
  Value                     NVARCHAR(100) NULL,
  Numeric_Value             DECIMAL(18,6) NULL,
  Unit
