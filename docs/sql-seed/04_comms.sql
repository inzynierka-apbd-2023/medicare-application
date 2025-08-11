-- Copied from dev-db/seed/04_comms.sql (truncated)
CREATE TABLE dbo.Conversation (
  Id               VARCHAR(36) NOT NULL CONSTRAINT DF_Conv_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Doctor_User_Id   VARCHAR(36) NOT NULL,
  Patient_User_Id  VARCHAR(36) NOT NULL,
  Subject          NVARCHAR(500) NULL,
  Created_At       DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  Last_Message_A   DATETIME NULL,
  Is_Active        BIT NOT NULL DEFAULT 1,
  CONSTRAINT PK_Conversation PRIMARY KEY (Id)
);
