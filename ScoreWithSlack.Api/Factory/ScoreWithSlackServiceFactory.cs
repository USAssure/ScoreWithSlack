using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScoreWithSlack.Service;
using System.Configuration;

namespace ScoreWithSlack.Api.Factory
{
    public interface IScoreWithSlackServiceFactory
    {
        ScoreWithSlackService CreateService();
    }

    public class ScoreWithSlackServiceFactory : IScoreWithSlackServiceFactory
    {
        public ScoreWithSlackService CreateService()
        {
            return new ScoreWithSlackService(new ScoreWithSlackConfig(ConfigurationManager.ConnectionStrings["ScoreWithSlackDb"].ToString()));
        }
    }
}