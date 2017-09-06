using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;

namespace Conch
{
    public static class WebApiConfig
    {
        public static string ConnectionString { get; set; }
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            var conStrs = System.Configuration.ConfigurationManager.ConnectionStrings;
            if (conStrs != null && conStrs.Count > 0)
            {
                var defaultConStr = conStrs["defaultConnection"];
                ConnectionString = defaultConStr.ConnectionString;
            }


            // 需要鉴权
            config.Filters.Add(new ApiAuthorizeAttribute());

            // Web API 路由
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "ActionRoute",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //    );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // configure json formatter
            JsonMediaTypeFormatter jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
