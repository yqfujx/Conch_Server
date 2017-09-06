using Conch.App_Start;
using System.Web.Http;
using System.Web.Mvc;

namespace Conch
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private PushTaskSchedule pushTask;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();

            pushTask = new PushTaskSchedule();
            pushTask.Start();
        }

    }
}
