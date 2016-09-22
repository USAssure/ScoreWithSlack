using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreWithSlack.Service
{
    public class ScoreWithSlackConfig
    {
        public string ConnectionString { get; set; }
        public int MaxScore { get; set; }

        public ScoreWithSlackConfig(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            MaxScore = 20000; //default
            ConnectionString = connectionString;
        }
    }
}
