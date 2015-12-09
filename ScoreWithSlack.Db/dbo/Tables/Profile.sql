CREATE TABLE [dbo].[Profile] (
    [ProfileId]     INT            IDENTITY (1, 1) NOT NULL,
    [SlackUserId]   VARCHAR (100)  NULL,
    [SlackUserName] VARCHAR (50)   NOT NULL,
    [FirstName]     NVARCHAR (50)  NULL,
    [LastName]      NVARCHAR (75)  NULL,
    [Email]         NVARCHAR (150) NULL,
    [JoinDate]      DATE           DEFAULT (sysdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProfileId] ASC)
);

