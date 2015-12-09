using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ScoreWithSlack.Api.MessageHandlers
{
    public class SlackTokenHandler : DelegatingHandler
    {
        private readonly string _scoreToken = ConfigurationManager.AppSettings["SlackScoreToken"];
        private readonly string _scoreboardToken = ConfigurationManager.AppSettings["SlackScoreboardToken"];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //grab the message sent from slack
            var message = await request.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

            //if a token or command isn't available, return a bad request response
            if (!json.ContainsKey("token") || !json.ContainsKey("command"))
                return request.CreateResponse(HttpStatusCode.OK, "Token or command is missing");

            //check if the command/token combination is valid; if so, continue
            if ((json["command"] == "/score" && json["token"] == _scoreToken) ||
                (json["command"] == "/scoreboard" && json["token"] == _scoreboardToken))
                return await base.SendAsync(request, cancellationToken);

            //invalid request, so return a forbidden response
            return request.CreateResponse(HttpStatusCode.OK, "Invalid command/token combination");
        }
    }
}