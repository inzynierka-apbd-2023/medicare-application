-- Copied from dev-db/seed/02_scheduling.sql (truncated)
CREATE TABLE dbo.Schedule (
  Id          VARCHAR(36) NOT NULL CONSTRAINT DF_Schedule_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Name        NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL,
  Created_At  DATETIME NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_Schedule PRIMARY KEY (Id)
);
