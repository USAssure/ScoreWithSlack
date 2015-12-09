using System;
using System.Collections.Generic;
using System.Linq;
using ScoreWithSlack.Entity;
using ScoreWithSlack.Models;
using ScoreWithSlack.Service.Adapters;

namespace ScoreWithSlack.Service
{
    //TODO: add achievements
    public class ScoreWithSlackService : IScoreWithSlackService
    {
        private readonly ScoreWithSlackEntities _context;
        private const int MaxSingleScore = 20000; //TODO: make configurable

        public ScoreWithSlackService(ScoreWithSlackEntities context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
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

            //select the most recent, active, season
            var slackTeamEntity =
                _context.SlackTeam.Single(
                    t => t.SlackTeamId == slackTeam.TeamId && t.SlackTeamToken == slackTeam.Token);

            //TODO: add multiple scorers
            var scoreForUser = scoreForSlackUsers.First();
            var scoreEntity = new Score
            {
                ScoreDate = DateTime.Now,
                Value = Math.Min(MaxSingleScore, scoreValues.Sum()),
                ScoreForProfile = _context.Profile.First(p => p.ProfileId == scoreForUser.ProfileId),
                ScoreFromProfile = _context.Profile.First(p => p.ProfileId == scoreFromSlackUser.ProfileId),
                Season = slackTeamEntity.Season.OrderByDescending(t => t.StartDate).First(),
                SlackTeam = slackTeamEntity
            };

            _context.Score.Add(scoreEntity);
            _context.SaveChanges();

            return ScoreWithSlackAdapter.ToScoreModel(scoreEntity);
        }

        public IEnumerable<ScoreModel> GetScoresForSlackTeam(SlackTeamModel slackTeam)
        {
            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            if (GetSlackTeam(slackTeam.Token, slackTeam.TeamId) == null)
                slackTeam = CreateTeam(slackTeam);

            var slackTeamEntity =
                _context.SlackTeam.Single(t => t.SlackTeamId == slackTeam.TeamId);

            //select the most recent, active, season
            var mostRecentSeason = slackTeamEntity.Season.OrderByDescending(s => s.StartDate).First();

            return
                _context.Score.Where(
                    s => s.SlackTeamId == slackTeamEntity.SlackTeamId && s.SeasonId == mostRecentSeason.SeasonId)
                    .ToList()
                    .Select(ScoreWithSlackAdapter.ToScoreModel);
        }

        private SlackTeamModel CreateTeam(SlackTeamModel slackTeam)
        {
            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            var slackTeamEntity = _context.SlackTeam.FirstOrDefault(t => t.SlackTeamId == slackTeam.TeamId);
            if (slackTeamEntity != null)
                return ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);

            //create new team and season
            slackTeamEntity = ScoreWithSlackAdapter.ToSlackTeamEntity(slackTeam);
            slackTeamEntity.JoinDate = DateTime.Now;
            slackTeamEntity.Season.Add(new Season
            {
                SeasonName = "Default",
                StartDate = DateTime.Now,
                EndDate = null
            });

            _context.SlackTeam.Add(slackTeamEntity);
            _context.SaveChanges();

            return ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);
        }

        private SlackTeamModel GetSlackTeam(string token, string teamId)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            if (string.IsNullOrEmpty(teamId))
                throw new ArgumentNullException(nameof(teamId));

            var slackTeamEntity =
                _context.SlackTeam.FirstOrDefault(t => t.SlackTeamId == teamId && t.SlackTeamToken == token);

            return slackTeamEntity == null ? null : ScoreWithSlackAdapter.ToSlackTeamModel(slackTeamEntity);
        }

        private SlackUserModel CreateSlackUser(SlackUserModel slackUser, SlackTeamModel slackTeam)
        {
            if (slackUser == null)
                throw new ArgumentNullException(nameof(slackUser));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            if (GetSlackTeam(slackTeam.Token, slackTeam.TeamId) == null)
                slackTeam = CreateTeam(slackTeam);

            var slackTeamEntity =
                _context.SlackTeam.Single(t => t.SlackTeamId == slackTeam.TeamId);

            //if the user exists, but isn't associated to the team, associate them; otherwise, return what was found
            var slackUserEntity = _context.Profile.FirstOrDefault(p => p.SlackUserName == slackUser.UserName);
            if (slackUserEntity != null)
                return slackUserEntity.SlackTeam.All(t => t.SlackTeamId != slackTeam.TeamId) ?
                            AssociateUserToTeam(ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity), slackTeam) :
                            ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);

            //create new user and associate them to the team
            slackUserEntity = ScoreWithSlackAdapter.ToProfileEntity(slackUser);
            slackUserEntity.JoinDate = DateTime.Now;
            slackUserEntity.SlackTeam.Add(slackTeamEntity);

            _context.Profile.Add(slackUserEntity);
            _context.SaveChanges();

            return ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
        }

        private SlackUserModel AssociateUserToTeam(SlackUserModel slackUser, SlackTeamModel slackTeam)
        {
            if (slackUser == null)
                throw new ArgumentNullException(nameof(slackUser));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            var slackUserEntity = _context.Profile.Single(p => p.SlackUserName == slackUser.UserName);
            var slackTeamEntity =
                _context.SlackTeam.Single(t => t.SlackTeamId == slackTeam.TeamId && t.SlackTeamToken == slackTeam.Token);

            slackUserEntity.SlackTeam.Add(slackTeamEntity);
            _context.SaveChanges();

            return ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
        }
        
        private SlackUserModel GetSlackUserForTeam(string userName, SlackTeamModel slackTeam)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (slackTeam == null)
                throw new ArgumentNullException(nameof(slackTeam));

            var slackUserEntity =
                _context.Profile.FirstOrDefault(
                    p => p.SlackUserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase) &&
                         p.SlackTeam.Any(t => t.SlackTeamId == slackTeam.TeamId && t.SlackTeamToken == slackTeam.Token));

            return slackUserEntity == null ? null : ScoreWithSlackAdapter.ToSlackUserModel(slackUserEntity);
        }
    }
}
