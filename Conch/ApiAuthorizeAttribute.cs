using Conch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Conch
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // 验证token
            //var token = actionContext.Request.Headers.Authorization;
            var ts = actionContext.Request.Headers.Where(c => c.Key.ToLower() == "token").FirstOrDefault().Value;
            if (ts != null && ts.Count() > 0)
            {
                var token = ts.First<string>();
                // 验证token
                if (!UserTokenManager.IsExistToken(token))
                {
                    return false;
                }
                return true;
            }

            if (actionContext.Request.Method == HttpMethod.Options)
                return true;
            return false;
        }
    }
}