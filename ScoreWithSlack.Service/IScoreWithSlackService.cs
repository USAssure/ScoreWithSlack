using System.Collections.Generic;
using ScoreWithSlack.Models;

namespace ScoreWithSlack.Service
{
    public interface IScoreWithSlackService
    {
        ScoreModel AddScore(
            SlackUserModel scoreFromUser, 
            IEnumerable<string> scoreForUsers,
            IEnumerable<int> scoreValues, 
            SlackTeamModel slackTeam);

        IEnumerable<ScoreModel> GetScoresForSlackTeam(SlackTeamModel slackTeam);
    }
}
