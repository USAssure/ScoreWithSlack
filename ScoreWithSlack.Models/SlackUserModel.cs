using System;

namespace ScoreWithSlack.Models
{
    public class SlackUserModel
    {
        public int ProfileId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
