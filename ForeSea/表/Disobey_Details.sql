﻿CREATE TABLE [dbo].[Disobey_Details]
(
	[GUID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(), 
    [Term] SMALLINT NOT NULL, 
    [SID] CHAR(9) NOT NULL, 
    [Staff] NCHAR(10) NOT NULL, 
    [Date] DATE NOT NULL DEFAULT GETDATE(), 
    [Type] SMALLINT NOT NULL, 
    [Detail] NTEXT NULL, 
    [State] NVARCHAR(50) NOT NULL, 
    [Note] NVARCHAR(50) NULL
)
