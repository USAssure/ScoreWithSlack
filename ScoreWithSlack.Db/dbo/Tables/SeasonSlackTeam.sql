CREATE TABLE [dbo].[SeasonSlackTeam] (
    [SeasonId]    INT           NOT NULL,
    [SlackTeamId] VARCHAR (100) NOT NULL,
    CONSTRAINT [Season_SeasonSlackTeam_FK] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Season] ([SeasonId]),
    CONSTRAINT [SlackTeam_SeasonSlackTeam_FK] FOREIGN KEY ([SlackTeamId]) REFERENCES [dbo].[SlackTeam] ([SlackTeamId]),
	CONSTRAINT [Season_SlackTeam_PK] PRIMARY KEY([SeasonId], [SlackTeamId])
);

