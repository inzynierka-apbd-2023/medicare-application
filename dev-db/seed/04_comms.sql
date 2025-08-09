CREATE TABLE dbo.Conversation (
  Id               VARCHAR(36) NOT NULL CONSTRAINT DF_Conv_Id DEFAULT dbo.NewGuidString(),
  Doctor_User_Id   VARCHAR(36) NOT NULL,
  Patient_User_Id  VARCHAR(36) NOT NULL,
  Subject          NVARCHAR(500) NULL,
  Created_At       DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Last_Message_A   DATETIME NULL,
  Is_Active        BIT NOT NULL DEFAULT 1,
  CONSTRAINT PK_Conversation PRIMARY KEY (Id),
  CONSTRAINT FK_Conv_DoctorUser FOREIGN KEY (Doctor_User_Id) REFERENCES dbo.[User](Id),
  CONSTRAINT FK_Conv_PatientUser FOREIGN KEY (Patient_User_Id) REFERENCES dbo.[User](Id)
);

CREATE TABLE dbo.Message (
  Id               VARCHAR(36) NOT NULL CONSTRAINT DF_Msg_Id DEFAULT dbo.NewGuidString(),
  Conversation_Id  VARCHAR(36) NOT NULL,
  Sender_Id        VARCHAR(36) NOT NULL,
  Sender_Name      NVARCHAR(200) NULL,
  Sender_Type      NVARCHAR(20)  NULL,
  Receiver_Id      VARCHAR(36) NULL,
  Receiver_Name    NVARCHAR(200) NULL,
  Receiver_Type    NVARCHAR(20)  NULL,
  Content          NVARCHAR(MAX) NOT NULL,
  Timestamp        DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Is_Read          BIT NOT NULL DEFAULT 0,
  Message_Type     NVARCHAR(50) NULL,
  Attachment_Path  NVARCHAR(1000) NULL,
  CONSTRAINT PK_Message PRIMARY KEY (Id),
  CONSTRAINT FK_Message_Conversation FOREIGN KEY (Conversation_Id) REFERENCES dbo.Conversation(Id)
);

CREATE TABLE dbo.Notification (
  Id               VARCHAR(36) NOT NULL CONSTRAINT DF_Notif_Id DEFAULT dbo.NewGuidString(),
  Recipient_User_Id VARCHAR(36) NOT NULL,
  Description      NVARCHAR(255) NULL,
  Type             TINYINT NULL,
  Creation_Date    DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Source_Service   NVARCHAR(64) NULL,
  Is_Read          BIT NOT NULL DEFAULT 0,
  Action_Url       NVARCHAR(500) NULL,
  Priority_Level   NVARCHAR(20) NULL,
  Expires_At       DATETIME NULL,
  CONSTRAINT PK_Notification PRIMARY KEY (Id),
  CONSTRAINT FK_Notification_User FOREIGN KEY (Recipient_User_Id) REFERENCES dbo.[User](Id)
);
