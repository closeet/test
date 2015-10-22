﻿CREATE TABLE [dbo].[CheckIn_Block] (
    [Name]      NCHAR (10) NOT NULL,
    [StartTime] TIME (0)   NOT NULL,
    [EndTime]   TIME (0)   NOT NULL,
    CONSTRAINT [PK_CheckIn_Block] PRIMARY KEY CLUSTERED ([Name] ASC)
);


