using System;

namespace ScoreWithSlack.Models
{
    public class SlackTeamModel
    {
        public string Token { get; set; }
        public string TeamId { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
