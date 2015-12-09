using System;

namespace ScoreWithSlack.Models
{
    public class ScoreModel
    {
        public int ScoreId { get; set; }
        public DateTime ScoreDate { get; set; }
        public int Value { get; set; }
        public SlackUserModel ScoreForUser { get; set; }
        public SlackUserModel ScoreFromUser { get; set; }
        public SlackTeamModel SlackTeam { get; set; }
        public SeasonModel Season { get; set; }
    }
}
