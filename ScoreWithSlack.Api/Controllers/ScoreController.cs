using System;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web.Http;
using ScoreWithSlack.Api.Models;
using ScoreWithSlack.Models;
using ScoreWithSlack.Service;

namespace ScoreWithSlack.Api.Controllers
{
    [RoutePrefix("score")]
    public class ScoreController : ApiController
    {
        private readonly IScoreWithSlackService _scoreWithSlackService;

        private const string ScoreCommand = "/score";
        private const string ScoreboardCommand = "/scoreboard"; 

        public ScoreController(IScoreWithSlackService scoreWithSlackService)
        {
            if(scoreWithSlackService == null)
                throw new ArgumentNullException(nameof(scoreWithSlackService));

            _scoreWithSlackService = scoreWithSlackService;
        }

        [HttpPost]
        [Route]
        public IHttpActionResult Post(SlackRequest request)
        {
            var slackTeam = new SlackTeamModel
            {
                Token = request.token,
                TeamId = request.team_id
            };

            var slackUser = new SlackUserModel
            {
                UserId = request.user_id,
                UserName = request.user_name
            };

            switch (request.command) //TODO: refactor
            {
                case ScoreCommand:
                    return string.IsNullOrEmpty(request.text) ? 
                        BadRequest("Usage: /score @user [value]") : 
                        ScoreRequest(slackUser, slackTeam, request.text);

                case ScoreboardCommand:
                    return ScoreboardRequest(slackUser, slackTeam);

                default:
                    return BadRequest("Unknown command");
            }
        }

        private IHttpActionResult ScoreRequest(SlackUserModel slackUser, SlackTeamModel slackTeam, string scoreCommand)
        {
            //parse the message request
            //TODO: make this more sophisticated
            var tokens = ParseText(scoreCommand);

            var scoreValues = tokens.Where(t =>
            {
                int tryParse;
                return int.TryParse(t, out tryParse);
            }).Select(int.Parse).ToArray();

            //must contain at least one mention and one integer
            if (!tokens.Any(t => t.Contains("@")) || !scoreValues.Any())
                return
                    BadRequest("At least one @ user mention and one integer is required.");

            var userTokens = tokens.Where(t => t.Contains("@")).Select(t => t.Remove(0, 1)).ToArray();

            //TODO: build a score analyzer
            //can't score yourself
            if (userTokens.Length == 1 && userTokens[0].Equals(slackUser.UserName))
                if (scoreValues.Sum() > 0)
                    return Ok(FormatResponse(
                        $"Hey everyone, @{slackUser.UserName} tried to boost their own score! SHAAAAME!",
                        true, null));
                else if (scoreValues.Sum() == 0)
                    return Ok(FormatResponse(
                        $"@{slackUser.UserName} doesn't know how this thing works. Someone wanna help them out? Anyone? No? Oh, well.",
                        true, null));
                else
                    return Ok(FormatResponse(
                        $"@{slackUser.UserName} tried to take points away from themselves. They so craaazy!",
                        true, null));

            //add the score
            var scoreModel = _scoreWithSlackService.AddScore(slackUser, userTokens,
                scoreValues, slackTeam);

            return Ok(FormatResponse(GetScoreAwardedMessage(scoreModel), true, null));
        }

        private IHttpActionResult ScoreboardRequest(SlackUserModel slackUser, SlackTeamModel slackTeam)
        {
            var scores = _scoreWithSlackService.GetScoresForSlackTeam(slackTeam).Select(s => new
            {
                s.ScoreDate,
                s.Value,
                ScoreForUserName = s.ScoreForUser.UserName,
                ScoreForUserId = s.ScoreForUser.UserId,
                ScoreFromUserId = s.ScoreFromUser.UserId,
                ScoreFromUserName = s.ScoreFromUser.UserName,
                s.SlackTeam.TeamId
            }).ToList();

            if (!scores.Any())
                return Ok(FormatResponse("No scores :(", false, null));

            //TODO: add additional categories (e.g. last 7 days, all time)
            var scoreGroups = scores.GroupBy(s => s.ScoreForUserName)
                .OrderByDescending(g => g.Sum(v => v.Value));

            var builder = new StringBuilder();
            var x = 1;

            foreach (var scoreGroup in scoreGroups)
            {
                builder.AppendFormat("{0}. @{1} ({2})\n", x++, scoreGroup.Key, scoreGroup.Sum(g => g.Value));
            }

            return Ok(FormatResponse(null, true, new[] { FormatAttachment("Scoreboard", $"Scoreboard requested by @{slackUser.UserName}", builder.ToString()) }));
        }

        private static string[] ParseText(string text)
        {
            if(string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            return text.Split(' ');
        }

        private static string GetScoreAwardedMessage(ScoreModel scoreModel)
        {
            const string format = "{0} awarded {1} points to {2}";
            var builder = new StringBuilder();
            builder.AppendFormat(format, scoreModel.ScoreFromUser.UserName, scoreModel.Value,
                scoreModel.ScoreForUser.UserName);

            if (scoreModel.Value >= 20000)
                builder.Append(". Holy schnikes! For everyone's protection, there's a limit of 20000!");
            else if (scoreModel.Value > 9000)
                builder.Append(". OVER 9000!? https://i.ytimg.com/vi/LqSg9yVfzV0/maxresdefault.jpg");
            else if (scoreModel.Value >= 1000)
                builder.Append(". Awesomeness warning!");
            else if (scoreModel.Value >= 500)
                builder.Append(". High 5's all around!");
            else if (scoreModel.Value >= 100)
                builder.Append(". Points for days!");
            else if (scoreModel.Value == 42)
                builder.Append(". The answer to all things.");
            else if (scoreModel.Value == 23)
                builder.Append(". http://images.sportsworldreport.com/data/images/full/15579/michael-jordan.jpg");
            else if (scoreModel.Value == 1337)
                builder.Append(". :panda_face: <= Not what you expected, huh?");
            else if (scoreModel.Value <= -10)
                builder.Append(". Uh oh! What's going on here?");
            else if (scoreModel.Value <= -100)
                builder.Append(". The points police have been called. This isn't good.");
            else if (scoreModel.Value <= -1000)
                builder.Append(". http://i.imgur.com/iWKad22.jpg");
            else if (scoreModel.Value <= -5000)
                builder.Append(". Stop. Just... stop.");

            return builder.ToString();
        }

        private static dynamic FormatAttachment(string title, string pretext, string content)
        {
            return new
            {
                title,
                pretext,
                text = content
            };
        }

        private static dynamic FormatResponse(string message, bool isVisible, dynamic[] attachments)
        {
            dynamic response = new ExpandoObject();

            if (attachments != null && attachments.Any())
                response.attachments = attachments;
            
            if(!string.IsNullOrEmpty(message))
                response.text = message;

            response.username = "scorebot";
            response.response_type = isVisible ? "in_channel" : "ephemeral";
            response.link_names = "1";
            response.parse = "full";

            return response;
        }
    }
}
