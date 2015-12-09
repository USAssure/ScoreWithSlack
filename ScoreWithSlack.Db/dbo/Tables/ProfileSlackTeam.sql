CREATE TABLE [dbo].[ProfileSlackTeam] (
    [ProfileId]   INT           NOT NULL,
    [SlackTeamId] VARCHAR (100) NOT NULL,
    CONSTRAINT [Profile_ProfileSlackTeam_FK] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[Profile] ([ProfileId]),
    CONSTRAINT [SlackTeam_ProfileSlackTeam_FK] FOREIGN KEY ([SlackTeamId]) REFERENCES [dbo].[SlackTeam] ([SlackTeamId]),
	CONSTRAINT [Profile_SlackTeam_PK] PRIMARY KEY ([ProfileId], [SlackTeamId])
);

