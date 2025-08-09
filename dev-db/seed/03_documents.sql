CREATE TABLE dbo.Document_Type (
  Id            VARCHAR(36) NOT NULL CONSTRAINT DF_DocType_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Code          NVARCHAR(30)  NOT NULL,
  Name          NVARCHAR(100) NOT NULL,
  Description   NVARCHAR(255) NULL,
  Template_Path NVARCHAR(500) NULL,
  CONSTRAINT PK_Document_Type PRIMARY KEY (Id)
);

CREATE TABLE dbo.Document (
  Id                VARCHAR(36) NOT NULL CONSTRAINT DF_Document_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Created_At        DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Notes             NVARCHAR(1000) NULL,
  Type              INT NULL,
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

CREATE TABLE dbo.Documents_Assigned (
  Id             VARCHAR(36) NOT NULL CONSTRAINT DF_DocAssigned_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Assigned_At    DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Appointment_Id VARCHAR(36) NOT NULL,
  Document_Id    VARCHAR(36) NOT NULL,
  CONSTRAINT PK_Documents_Assigned PRIMARY KEY (Id),
  CONSTRAINT UQ_Documents_Assigned UNIQUE (Appointment_Id, Document_Id),
  CONSTRAINT FK_DA_Appointment FOREIGN KEY (Appointment_Id) REFERENCES dbo.Appointment(Id),
  CONSTRAINT FK_DA_Document FOREIGN KEY (Document_Id) REFERENCES dbo.Document(Id)
);

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

CREATE TABLE dbo.Lab_Test_Type (
  Id                VARCHAR(36) NOT NULL CONSTRAINT DF_LabTestType_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
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
  Id                        VARCHAR(36) NOT NULL CONSTRAINT DF_LabTestResult_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Lab_Results_Document_I    VARCHAR(36) NOT NULL,
  Lab_Test_Type_Id          VARCHAR(36) NOT NULL,
  Parameter_Name            NVARCHAR(200) NULL,
  Value                     NVARCHAR(100) NULL,
  Numeric_Value             DECIMAL(18,6) NULL,
  Unit                      NVARCHAR(50) NULL,
  Reference_Range           NVARCHAR(200) NULL,
  Status                    NVARCHAR(50) NULL,
  Notes                     NVARCHAR(1000) NULL,
  Is_Abnormal               BIT NULL,
  CONSTRAINT PK_Lab_Test_Result PRIMARY KEY (Id),
  CONSTRAINT FK_LTR_LabResults FOREIGN KEY (Lab_Results_Document_I) REFERENCES dbo.Lab_Results(Document_Id),
  CONSTRAINT FK_LTR_TestType FOREIGN KEY (Lab_Test_Type_Id) REFERENCES dbo.Lab_Test_Type(Id)
);

CREATE TABLE dbo.Prescription (
  Document_I    VARCHAR(36) NOT NULL,
  Medication    NVARCHAR(200) NOT NULL,
  Dosage        NVARCHAR(100) NOT NULL,
  Frequency     NVARCHAR(100) NULL,
  Duration_Da   INT NULL,
  Instructions  NVARCHAR(MAX) NULL,
  Pharmacy_N    NVARCHAR(200) NULL,
  Pharmacy_P    NVARCHAR(20)  NULL,
  Refills_Rema  INT NULL,
  CONSTRAINT PK_Prescription PRIMARY KEY (Document_I),
  CONSTRAINT FK_Prescription_Document FOREIGN KEY (Document_I) REFERENCES dbo.Document(Id)
);

CREATE TABLE dbo.Referral (
  Document      VARCHAR(36) NOT NULL,
  Speciality    NVARCHAR(100) NOT NULL,
  Referred_     NVARCHAR(255) NULL,
  Valid_Fro     DATETIME NULL,
  Valid_To      DATETIME NULL,
  Reason        NVARCHAR(1000) NULL,
  Urgency_L     NVARCHAR(50) NULL,
  CONSTRAINT PK_Referral PRIMARY KEY (Document),
  CONSTRAINT FK_Referral_Document FOREIGN KEY (Document) REFERENCES dbo.Document(Id)
);

CREATE TABLE dbo.Sick_Leave (
  Document_Id        VARCHAR(36) NOT NULL,
  Start_Date         DATETIME NOT NULL,
  End_Date           DATETIME NOT NULL,
  Days_Off           INT NULL,
  Return_To_Work_    DATETIME NULL,
  Work_Restrictions  NVARCHAR(1000) NULL,
  CONSTRAINT PK_Sick_Leave PRIMARY KEY (Document_Id),
  CONSTRAINT FK_SickLeave_Document FOREIGN KEY (Document_Id) REFERENCES dbo.Document(Id)
);
