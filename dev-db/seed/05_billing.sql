CREATE TABLE dbo.Subscription_Payment (
  Id                 VARCHAR(36) NOT NULL CONSTRAINT DF_SubPay_Id DEFAULT dbo.NewGuidString(),
  Amount             DECIMAL(10,2) NOT NULL,
  Currency           NVARCHAR(10) NOT NULL,
  Status             NVARCHAR(32) NULL,
  Paid_At            DATETIME NULL,
  Renewal_Date       DATETIME NULL,
  Patient_Id         VARCHAR(36) NULL,
  Subscription_Type  NVARCHAR(50) NULL,
  Payment_Method     NVARCHAR(100) NULL,
  Transaction_Id     NVARCHAR(200) NULL,
  CONSTRAINT PK_Subscription_Payment PRIMARY KEY (Id),
  CONSTRAINT FK_SubPay_Patient FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id)
);

CREATE TABLE dbo.Appointment_Payment (
  Id                      VARCHAR(36) NOT NULL CONSTRAINT DF_AppPay_Id DEFAULT dbo.NewGuidString(),
  Amount                  DECIMAL(10,2) NOT NULL,
  Currency                NVARCHAR(10) NOT NULL,
  Status                  NVARCHAR(32) NULL,
  Paid_At                 DATETIME NULL,
  Renewal_Date            DATETIME NULL,
  Schedule_Appointment_Id VARCHAR(36) NULL,
  Patient_Id              VARCHAR(36) NULL,
  Payment_Method          NVARCHAR(100) NULL,
  Transaction_Id          NVARCHAR(200) NULL,
  CONSTRAINT PK_Appointment_Payment PRIMARY KEY (Id),
  CONSTRAINT FK_AppPay_SA FOREIGN KEY (Schedule_Appointment_Id) REFERENCES dbo.Schedule_Appointment(Id),
  CONSTRAINT FK_AppPay_Patient FOREIGN KEY (Patient_Id) REFERENCES dbo.Patient(Id)
);
