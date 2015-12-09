CREATE TABLE [dbo].[SlackTeam] (
    [SlackTeamId]    VARCHAR (100)  NOT NULL,
    [SlackTeamToken] NVARCHAR (150) NOT NULL,
    [JoinDate]       DATE           DEFAULT (sysdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([SlackTeamId] ASC)
);

