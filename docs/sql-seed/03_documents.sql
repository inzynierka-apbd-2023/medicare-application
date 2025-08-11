-- Copied from dev-db/seed/03_documents.sql (truncated)
CREATE TABLE dbo.Document_Type (
  Id            VARCHAR(36) NOT NULL CONSTRAINT DF_DocType_Id DEFAULT CONVERT(VARCHAR(36), NEWID()),
  Code          NVARCHAR(30)  NOT NULL,
  Name          NVARCHAR(100) NOT NULL,
  Description   NVARCHAR(255) NULL,
  Template_Path NVARCHAR(500) NULL,
  CONSTRAINT PK_Document_Type PRIMARY KEY (Id)
);
