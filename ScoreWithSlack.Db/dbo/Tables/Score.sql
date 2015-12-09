CREATE TABLE [dbo].[Score] (
    [ScoreId]            INT           IDENTITY (1, 1) NOT NULL,
    [ScoreDate]          DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    [Value]              INT           NOT NULL,
    [ScoreForProfileId]  INT           NOT NULL,
    [ScoreFromProfileId] INT           NOT NULL,
    [SeasonId]           INT           NOT NULL,
    [SlackTeamId]        VARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([ScoreId] ASC),
    CONSTRAINT [Profile_ScoreFor_FK] FOREIGN KEY ([ScoreForProfileId]) REFERENCES [dbo].[Profile] ([ProfileId]),
    CONSTRAINT [Profile_ScoreFrom_FK] FOREIGN KEY ([ScoreFromProfileId]) REFERENCES [dbo].[Profile] ([ProfileId]),
    CONSTRAINT [Season_Score_FK] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Season] ([SeasonId]),
    CONSTRAINT [SlackTeam_Score_FK] FOREIGN KEY ([SlackTeamId]) REFERENCES [dbo].[SlackTeam] ([SlackTeamId])
);

