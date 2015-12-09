using System.Data.Entity;
using System.Web.Http;
using Microsoft.Practices.Unity;
using ScoreWithSlack.Api.IoC;
using ScoreWithSlack.Api.MessageHandlers;
using ScoreWithSlack.Entity;
using ScoreWithSlack.Service;

namespace ScoreWithSlack.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //configure unity
            var container = new UnityContainer();
            container.RegisterType<DbContext, ScoreWithSlackEntities>(new HierarchicalLifetimeManager());
            container.RegisterType<IScoreWithSlackService, ScoreWithSlackService>();
            config.DependencyResolver = new UnityDependencyResolver(container);

            //TODO: debug on azure; works fine localhost
            //configure handlers
            //config.MessageHandlers.Add(new SlackTokenHandler());

            //configure routes
            config.MapHttpAttributeRoutes();
        }
    }
}
