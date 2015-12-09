using System;

namespace ScoreWithSlack.Models
{
    public class SeasonModel
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
