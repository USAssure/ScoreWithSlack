CREATE TABLE [dbo].[Season] (
    [SeasonId]   INT            IDENTITY (1, 1) NOT NULL,
    [SeasonName] NVARCHAR (100) NOT NULL,
    [StartDate]  DATETIME2 (7)  NOT NULL,
    [EndDate]    DATETIME2 (7)  NULL,
    PRIMARY KEY CLUSTERED ([SeasonId] ASC)
);

