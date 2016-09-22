using System;
using System.Collections.Generic;
using System.Linq;
using ScoreWithSlack.Entity;
using ScoreWithSlack.Models;
using ScoreWithSlack.Service.Adapters;
using System.Data.SqlClient;
using Dapper;

namespace ScoreWithSlack.Service
{
    //TODO: add achievements
    public class ScoreWithSlackService : IScoreWithSlackService
    {
        private readonly ScoreWithSlackConfig _config;
        private const int MaxSingleScore = 20000; //TODO: make configurable

        public ScoreWithSlackService(ScoreWithSlackConfig config)
        {
            if(config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config;
        }

        //TODO: hook into slack api to validate users
        public ScoreModel AddScore(
            SlackUserModel scoreFromUser, 
            IEnumerable<string> scoreForUsers,
            IEnumerable<int> scoreValues, 
            SlackTeamModel slackTeam)
        {
            //get the team; create it if it doesn't exist
            slackTeam = GetSlackTeam(slackTeam.Token, slackTeam.TeamId) ??
                        CreateTeam(slackTeam);

            //get score from user; create and associate them if they don't exist
            var scoreFromSlackUser =
                GetSlackUserForTeam(scoreFromUser.UserName, slackTeam) ??
                CreateSlackUser(new SlackUserModel
                {
                    UserName = scoreFromUser.UserName
                }, slackTeam);

            //get all of the users; create and associate them if they don't exist
            var scoreForSlackUsers =
                scoreForUsers.Select(scoreForSlackUser =>
                    GetSlackUserForTeam(scoreForSlackUser, slackTeam) ??
                    CreateSlackUser(new SlackUserModel
                    {
                        UserName = scoreForSlackUser
                    }, slackTeam)).ToList();

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var scoreEntity = connection.Query<Score, Profile, Profile, Season, SlackTeam, Score>(
                    @"declare @SeasonId int
                      declare @ScoreResult as table (ScoreId int)

                      select top 1 @SeasonId = s.[SeasonId]
                      from   [ScoreWithSlack.Db].[dbo].[Season] s
                          inner join [ScoreWithSlack.Db].[dbo].[SeasonSlackTeam] sst on s.[SeasonId] = sst.[SeasonId]
                      where  sst.[SlackTeamId] = @teamId
                      order by s.[StartDate] desc;

                      insert into [dbo].[Score] ([ScoreDate], [Value], [ScoreForProfileId], [ScoreFromProfileId], [SeasonId], [SlackTeamId]) output Inserted.ScoreId into @ScoreResult
                                         values (@scoreDate, @value, @scoreForProfileId, @scoreFromProfileId, @SeasonId, @teamId);

                      select s.*
                           , fp.[ProfileId] as 'ScoreForProfileId'
	                       , fp.[SlackUserId]
	                       , fp.[SlackUserName]
	                       , fp.[FirstName]
	                       , fp.[LastName]
	                       , fp.[Email]
	                       , fp.[JoinDate]
                           , frp.[ProfileId] as 'ScoreFromProfileId'
	                       , frp.[SlackUserId]
	                       , frp.[SlackUserName]
	                       , frp.[FirstName]
	                       , frp.[LastName]
	                       , frp.[Email]
	                       , frp.[JoinDate]
	                       , sn.[SeasonId] as 'ScoreSeasonId'
	                       , sn.[SeasonName]
	                       , sn.[StartDate]
	                       , sn.[EndDate]
	                       , st.[SlackTeamId] as 'ScoreSlackTeamId'
	                       , st.[SlackTeamToken]
	                       , st.[JoinDate]
                      from   [ScoreWithSlack.Db].[dbo].[Score] s
                          inner join @ScoreResult sr on s.[ScoreId] = sr.[ScoreId]
                          inner join [ScoreWithSlack.Db].[dbo].[Profile] fp on s.[ScoreForProfileId] = fp.[ProfileId]
	                      inner join [ScoreWithSlack.Db].[dbo].[Profile] frp on s.[ScoreFromProfileId] = frp.[ProfileId]
	                      inner join [ScoreWithSlack.Db].[dbo].[Season] sn on s.[SeasonId] = sn.[SeasonId]
	                      inner join [ScoreWithSlack.Db].[dbo].[SlackTeam] st on s.[SlackTeamId] = st.[SlackTeamId];",
                    map: (score, forProfile, fromProfile, season, team) =>
                    {
                        score.ScoreForProfile = forProfile;
                        score.ScoreFromProfile = fromProfile;
                        score.Season = season;
                        score.SlackTeam = team;
                        return score;
                    },
                    splitOn: "ScoreForProfileId,ScoreFromProfileId,ScoreSeasonId,ScoreSlackTeamId",
                    param: new
                    {
                        teamId = slackTeam.TeamId,
                        scoreDate = DateTime.Now,
                        value = Math.Min(MaxSingleScore, scoreValues.Sum()),
                        scoreForProfileId = scoreForSlackUsers.First().ProfileId,
                        scoreFromProfileId = scoreFromSlackUser.ProfileId
                    }).Single();

                return ScoreWithSlackAdapter.ToScoreModel(scoreEntity);
            }
        }

        public IEnumerable<ScoreModel> GetScoresForSlackTeam(SlackTeamModel slackTeam)
        {
            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            if (GetSlackTeam(slackTeam.Token, slackTeam.TeamId) == null)
                slackTeam = CreateTeam(slackTeam);

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                return connection.Query<Score, Profile, Profile, Season, SlackTeam, Score>(
                    @"declare @SeasonId int

                    select top 1 @SeasonId = s.[SeasonId] 
                    from   [ScoreWithSlack.Db].[dbo].[Season] s
	                    inner join [ScoreWithSlack.Db].[dbo].[SeasonSlackTeam] sst on s.[SeasonId] = sst.[SeasonId]
                    where  sst.[SlackTeamId] = 'T03G1BGT1'
                    order by s.[StartDate] desc;

                    select s.*
                         , fp.[ProfileId] as 'ScoreForProfileId'
	                     , fp.[SlackUserId]
	                     , fp.[SlackUserName]
	                     , fp.[FirstName]
	                     , fp.[LastName]
	                     , fp.[Email]
	                     , fp.[JoinDate]
                         , frp.[ProfileId] as 'ScoreFromProfileId'
	                     , frp.[SlackUserId]
	                     , frp.[SlackUserName]
	                     , frp.[FirstName]
	                     , frp.[LastName]
	                     , frp.[Email]
	                     , frp.[JoinDate]
	                     , sn.[SeasonId] as 'ScoreSeasonId'
	                     , sn.[SeasonName]
	                     , sn.[StartDate]
	                     , sn.[EndDate]
	                     , st.[SlackTeamId] as 'ScoreSlackTeamId'
	                     , st.[SlackTeamToken]
	                     , st.[JoinDate]
                    from   [ScoreWithSlack.Db].[dbo].[Score] s
	                    inner join [ScoreWithSlack.Db].[dbo].[Profile] fp on s.[ScoreForProfileId] = fp.[ProfileId]
	                    inner join [ScoreWithSlack.Db].[dbo].[Profile] frp on s.[ScoreFromProfileId] = frp.[ProfileId]
	                    inner join [ScoreWithSlack.Db].[dbo].[Season] sn on s.[SeasonId] = sn.[SeasonId]
	                    inner join [ScoreWithSlack.Db].[dbo].[SlackTeam] st on s.[SlackTeamId] = st.[SlackTeamId]
                    where  s.[SlackTeamId] = 'T03G1BGT1'
                       and s.[SeasonId] = @SeasonId;",
                    map: (score, forProfile, fromProfile, season, team) =>
                    {
                        score.ScoreForProfile = forProfile;
                        score.ScoreFromProfile = fromProfile;
                        score.Season = season;
                        score.SlackTeam = team;
                        return score;
                    }, 
                    splitOn: "ScoreForProfileId,ScoreFromProfileId,ScoreSeasonId,ScoreSlackTeamId",
                    param: new { teamId = slackTeam.TeamId })
                    .ToList().Select(ScoreWithSlackAdapter.ToScoreModel);
            }
        }

        private SlackTeamModel CreateTeam(SlackTeamModel slackTeam)
        {
            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var slackTeamEntity = connection.Query<SlackTeam>(
                    @"select *
                      from   [dbo].[SlackTeam]
                      where  [SlackTeamId] = @teamId", new { teamId = slackTeam.TeamId }).FirstOrDefault();

                if (slackTeamEntity != null)
                    return ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);

                //create new team and season
                slackTeamEntity = ScoreWithSlackAdapter.ToSlackTeamEntity(slackTeam);
                slackTeamEntity.JoinDate = DateTime.Now;

                var slackTeamSeason = new Season
                {
                    SeasonName = "Default",
                    StartDate = DateTime.Now,
                    EndDate = null
                };

                connection.Execute(
                    @"insert into [dbo].[SlackTeam] values (@slackTeamId, @slackTeamToken, @joinDate", 
                    new { slackTeamId = slackTeam.TeamId, slackTeamToken = slackTeam.Token, joinDate = slackTeam.JoinDate });

                connection.Execute(
                    @"insert into [dbo].[SeasonSlackTeam] ([SlackTeamId]) values (@slackTeamId)",
                    new { slackTeamId = slackTeam.TeamId });

                return ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);
            }
        }

        private SlackTeamModel GetSlackTeam(string token, string teamId)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            if (string.IsNullOrEmpty(teamId))
                throw new ArgumentNullException(nameof(teamId));

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var slackTeamEntity = connection.Query<SlackTeam>(
                    @"select *
                      from   [dbo].[SlackTeam]
                      where  [SlackTeamId] = @teamId
                         and [SlackTeamToken] = @token"
                , new { teamId = teamId, token = token }).FirstOrDefault();

                return slackTeamEntity == null ? null : ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);
            }
        }

        private SlackUserModel CreateSlackUser(SlackUserModel slackUser, SlackTeamModel slackTeam)
        {
            if (slackUser == null)
                throw new ArgumentNullException(nameof(slackUser));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            if (GetSlackTeam(slackTeam.Token, slackTeam.TeamId) == null)
                slackTeam = CreateTeam(slackTeam);

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var slackTeamEntity = connection.Query<SlackTeam>(
                    @"select *
                      from   [dbo].[SlackTeam]
                      where  [SlackTeamId] = @teamId",
                    new { teamId = slackTeam.TeamId }).Single();

                var slackUserEntity = connection.Query<Profile>(
                    @"select *
                      from   [dbo].[Profile]
                      where  [SlackUserName] = @userName",
                    new { userName = slackUser.UserName }).FirstOrDefault();

                var userSlackTeams = connection.Query(
                    @"select *
                      from   [dbo].[ProfileSlackTeam]
                      where  [SlackTeamId] = @teamId",
                    new { teamId = slackTeam.TeamId });

                //if the user exists, but isn't associated to the team, associate them; otherwise, return what was found
                if (slackUserEntity != null)
                    return !userSlackTeams.Any() ?
                                AssociateUserToTeam(ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity), slackTeam) :
                                ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);

                //create new user and associate them to the team
                slackUserEntity = ScoreWithSlackAdapter.ToProfileEntity(slackUser);
                slackUserEntity.JoinDate = DateTime.Now;

                int profileId = connection.Query<int>(
                    @"insert into [dbo].[Profile] ([SlackUserId], [SlackUserName], [FirstName], [LastName], [Email], [JoinDate])
                                           values (@userId, @userName, @firstName, @lastName, @email, @joinDate);

                      select cast(scope_identity() as int)",
                    new
                    {
                        userId = slackUserEntity.SlackUserId,
                        userName = slackUserEntity.SlackUserName,
                        firstName = slackUserEntity.FirstName,
                        lastName = slackUserEntity.LastName,
                        email = slackUserEntity.Email,
                        joinDate = slackUserEntity.JoinDate
                    }).Single();

                connection.Execute(
                    @"insert into [dbo].[ProfileSlackTeam] values (@profileId, @teamId)",
                    new { profileId = profileId, teamId = slackTeamEntity.SlackTeamId });

                return ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
            }
        }

        private SlackUserModel AssociateUserToTeam(SlackUserModel slackUser, SlackTeamModel slackTeam)
        {
            if (slackUser == null)
                throw new ArgumentNullException(nameof(slackUser));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var slackUserEntity = connection.Query<Profile>(
                    @"select *
                      from   [dbo].[Profile]
                      where  [SlackUserName] = @userName",
                    new {  userName = slackUser.UserName}).Single();

                var slackTeamEntity = connection.Query<SlackTeam>(
                    @"select *
                      from   [dbo].[SlackTeam]
                      where  [SlackTeamId] = @teamId
                         and [SlackTeamToken] = @token",
                    new { teamId = slackTeam.TeamId, token = slackTeam.Token }).Single();

                connection.Execute(
                    @"insert into [dbo].[ProfileSlackTeam] values (@profileId, @teamId)",
                    new { profileId = slackUserEntity.ProfileId, teamId = slackTeamEntity.SlackTeamId });

                return ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
            }
        }
        
        private SlackUserModel GetSlackUserForTeam(string userName, SlackTeamModel slackTeam)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                var slackUserEntity = connection.Query<Profile>(
                        @"select p.*
                          from   [ScoreWithSlack.Db].[dbo].[Profile] p
	                          inner join [ScoreWithSlack.Db].[dbo].[ProfileSlackTeam] pst on p.[ProfileId] = pst.[ProfileId]
	                          inner join [ScoreWithSlack.Db].[dbo].[SlackTeam] st on pst.[SlackTeamId] = st.[SlackTeamId]
                          where  p.[SlackUserName] = @userName
                             and st.[SlackTeamId] = @teamId
                             and st.[SlackTeamToken] = @token;",
                        new { userName = userName, teamId = slackTeam.TeamId, token = slackTeam.Token }
                    ).FirstOrDefault();

                return slackUserEntity == null ? null : ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
            }
        }
    }
}
