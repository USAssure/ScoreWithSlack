using System;
using ScoreWithSlack.Entity;
using ScoreWithSlack.Models;

namespace ScoreWithSlack.Service.Adapters
{
    public static class ScoreWithSlackAdapter
    {
        #region Profile
        public static Profile ToProfileEntity(SlackUserModel model)
        {
            return new Profile
            {
                ProfileId = model.ProfileId,
                SlackUserId = model.UserId,
                SlackUserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                JoinDate = model.JoinDate
            };
        }

        public static SlackUserModel ToSlackUserModel(Profile entity)
        {
            return new SlackUserModel
            {
                ProfileId = entity.ProfileId,
                UserId = entity.SlackUserId,
                UserName = entity.SlackUserName,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                JoinDate = entity.JoinDate
            };
        }
        #endregion

        #region SlackTeam
        public static SlackTeam ToSlackTeamEntity(SlackTeamModel model)
        {
            return new SlackTeam
            {
                SlackTeamId = model.TeamId,
                SlackTeamToken = model.Token,
                JoinDate = model.JoinDate
            };
        }

        public static SlackTeamModel ToSlackTeamModel(SlackTeam entity)
        {
            return new SlackTeamModel
            {
                TeamId = entity.SlackTeamId,
                Token = entity.SlackTeamToken,
                JoinDate = entity.JoinDate
            };
        }
        #endregion

        #region Season
        public static Season ToSeasonEntity(SeasonModel model)
        {
            return new Season
            {
                SeasonId = model.SeasonId,
                SeasonName = model.SeasonName,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
        }

        public static SeasonModel ToSeasonModel(Season entity)
        {
            return new SeasonModel
            {
                SeasonId = entity.SeasonId,
                SeasonName = entity.SeasonName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };
        }
        #endregion

        #region Score
        public static Score ToScoreEntity(ScoreModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            return new Score
            {
                ScoreId = model.ScoreId,
                ScoreDate = model.ScoreDate,
                Value = model.Value,
                ScoreForProfileId = model.ScoreForUser?.ProfileId ?? 0,
                ScoreFromProfileId = model.ScoreFromUser?.ProfileId ?? 0,
                SeasonId = model.Season?.SeasonId ?? 0,
                SlackTeamId = model.SlackTeam?.TeamId ?? "default"
            };
        }

        public static ScoreModel ToScoreModel(Score entity)
        {
            return new ScoreModel
            {
                ScoreId = entity.ScoreId,
                ScoreDate = entity.ScoreDate,
                Value = entity.Value,
                ScoreForUser = ToSlackUserModel(entity.ScoreForProfile),
                ScoreFromUser = ToSlackUserModel(entity.ScoreFromProfile),
                Season = ToSeasonModel(entity.Season),
                SlackTeam = ToSlackTeamModel(entity.SlackTeam)
            };
        }
        #endregion
    }
}
