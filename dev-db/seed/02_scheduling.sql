CREATE TABLE dbo.Schedule (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_Schedule_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name        NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL,
  Created_At  DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Schedule PRIMARY KEY (Id)
);

ALTER TABLE dbo.[User]
  ADD CONSTRAINT FK_User_Schedule FOREIGN KEY (Schedule_Id) REFERENCES dbo.Schedule(Id);

CREATE TABLE dbo.Schedule_Appointment_Status (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_SAStatus_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name        NVARCHAR(100) NOT NULL,
  Description NVARCHAR(500) NULL,
  Color_Code  NVARCHAR(7) NULL,
  CONSTRAINT PK_Schedule_Appointment_Status PRIMARY KEY (Id),
  CONSTRAINT CK_SAStatus_ColorCode CHECK (Color_Code IS NULL OR LEN(Color_Code)=7)
);

CREATE TABLE dbo.Time_Slot (
  Id           VARCHAR(36) NOT NULL CONSTRAINT DF_TimeSlot_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Doctor_Id    VARCHAR(36) NOT NULL,
  Start_DateT  DATETIME NOT NULL,
  End_DateTi   DATETIME NOT NULL,
  Is_Available BIT NOT NULL DEFAULT 1,
  Duration_Mi  INT NOT NULL,
  Slot_Type    NVARCHAR(50) NULL,
  Created_At   DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Time_Slot PRIMARY KEY (Id),
  CONSTRAINT FK_TimeSlot_Doctor FOREIGN KEY (Doctor_Id) REFERENCES dbo.Doctor(Id)
);

CREATE TABLE dbo.Doctor_Schedule (
  Id           VARCHAR(36) NOT NULL CONSTRAINT DF_DoctorSchedule_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Doctor_Id    VARCHAR(36) NOT NULL,
  Day_Of_W     TINYINT NOT NULL,
  Start_Time   TIME NULL,
  End_Time     TIME NULL,
  Is_Availabl  BIT NOT NULL DEFAULT 1,
  Valid_Fro    DATE NULL,
  Valid_To     DATE NULL,
  Break_Star   TIME NULL,
  Break_End    TIME NULL,
  CONSTRAINT PK_Doctor_Schedule PRIMARY KEY (Id),
  CONSTRAINT FK_DoctorSchedule_Doctor FOREIGN KEY (Doctor_Id) REFERENCES dbo.Doctor(Id)
);

CREATE TABLE dbo.Schedule_Appointment (
  Id                             VARCHAR(36) NOT NULL CONSTRAINT DF_SA_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Schedule_Id                    VARCHAR(36) NOT NULL,
  Time_Slot_Id                   VARCHAR(36) NULL,
  Day                            DATETIME NOT NULL,
  Duration_Minutes               INT NOT NULL,
  Room                           NVARCHAR(255) NULL,
  Description                    NVARCHAR(1000) NULL,
  Appointment_Type               NVARCHAR(50) NULL,
  Doctor_User_Id                 VARCHAR(36) NOT NULL,
  Patient_User_Id                VARCHAR(36) NOT NULL,
  Receptionist_User_Id           VARCHAR(36) NULL,
  Schedule_Appointment_Status_I  VARCHAR(36) NULL,
  Total_Cost                     DECIMAL(10,2) NULL,
  Created_At                     DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Updated_At                     DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Schedule_Appointment PRIMARY KEY (Id),
  CONSTRAINT FK_SA_Schedule FOREIGN KEY (Schedule_Id) REFERENCES dbo.Schedule(Id),
  CONSTRAINT FK_SA_TimeSlot FOREIGN KEY (Time_Slot_Id) REFERENCES dbo.Time_Slot(Id),
  CONSTRAINT FK_SA_DoctorUser FOREIGN KEY (Doctor_User_Id) REFERENCES dbo.[User](Id),
  CONSTRAINT FK_SA_PatientUser FOREIGN KEY (Patient_User_Id) REFERENCES dbo.[User](Id),
  CONSTRAINT FK_SA_ReceptionistUser FOREIGN KEY (Receptionist_User_Id) REFERENCES dbo.[User](Id),
  CONSTRAINT FK_SA_Status FOREIGN KEY (Schedule_Appointment_Status_I) REFERENCES dbo.Schedule_Appointment_Status(Id)
);
CREATE INDEX IX_SA_DoctorUser  ON dbo.Schedule_Appointment(Doctor_User_Id);
CREATE INDEX IX_SA_PatientUser ON dbo.Schedule_Appointment(Patient_User_Id);

CREATE TABLE dbo.Appointment (
  Id                      VARCHAR(36) NOT NULL CONSTRAINT DF_Appointment_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Schedule_Appointment_I  VARCHAR(36) NOT NULL,
  Description             NVARCHAR(1000) NULL,
  Created_At              DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Appointment PRIMARY KEY (Id),
  CONSTRAINT FK_Appointment_SA FOREIGN KEY (Schedule_Appointment_I) REFERENCES dbo.Schedule_Appointment(Id)
);
