using Conch.Common;
using Conch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Conch
{
    /// <summary>
    /// 用户令牌验证
    /// </summary>
    public class TokenAuthorizeAttribute : ActionFilterAttribute
    {
        private const string UserToken = "token";
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // 匿名访问验证
            var anonymousAction = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>();
            if (!anonymousAction.Any())
            {
                // 验证token
                var token = TokenVerification(actionContext);
            }
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// 身份令牌验证
        /// </summary>
        /// <param name="actionContext"></param>
        protected virtual string TokenVerification(HttpActionContext actionContext)
        {
            // 获取token
            var token = GetToken(actionContext);
            if (string.IsNullOrEmpty(token))
            {
                actionContext.Response = actionContext.Request.CreateResponse<ResultData>(System.Net.HttpStatusCode.OK, 
                    new ResultData(ResultType.Unauthorized, EnumExtension.GetEnumDescription(ResultType.Unauthorized)));
            }
            // 判断token是否有效
            else if (!UserTokenManager.IsExistToken(token))
            {
                actionContext.Response = actionContext.Request.CreateResponse<ResultData>(System.Net.HttpStatusCode.OK,
                    new ResultData(ResultType.Unauthorized, "已过期，请重新登录"));
            }

            return token;
        }

        private string GetToken(HttpActionContext actionContext)
        {
            Dictionary<string, object> actionArguments = actionContext.ActionArguments;
            HttpMethod type = actionContext.Request.Method;
            var token = "";

            if (actionArguments.ContainsKey(UserToken))
            {
                if (actionArguments[UserToken] != null)
                {
                    token = actionArguments[UserToken].ToString();
                }
            }
            else
            {
                foreach (var value in actionArguments.Values)
                {
                    if (value != null && value.GetType().GetProperty(UserToken) != null)
                    {
                        token = value.GetType().GetProperty(UserToken).GetValue(value, null).ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(token) && actionContext.Request.Method == HttpMethod.Get)
            {
                var dictionary = actionContext.Request.GetQueryStrings();
                if (dictionary.ContainsKey(UserToken))
                {
                    token = dictionary[UserToken];
                }
            }
            return token;
        }
    }
}