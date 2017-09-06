using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Conch.Models;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Configuration;
using Conch.Common;

namespace Conch.Controllers
{
    /// <summary>
    /// 账户
    /// </summary>
    public class AccountController : ApiController
    {
        /// POST: api/account/login
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">登录人员信息： 账号，密码 ，是否记住密码</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ResultData Login(LoginUser login)
        {
            if (login == null)
            {
                return new ResultData((int)HttpStatusCode.BadRequest, "参数错误");
            }

            string userID = login.UserID;
            string password = login.Password;

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password))
            {
                return new ResultData(ResultType.UserNameOrPasswordError, EnumExtension.GetEnumDescription(ResultType.UserNameOrPasswordError));
            }

            User u = null;
            ResultType loginResult = UserManager.Login(login, out u);
            ResultData resultData = new ResultData
            {
                Result = Convert.ToInt32(loginResult),
                Desc = EnumExtension.GetEnumDescription(loginResult)
            };
            if (ResultType.Success == loginResult)
            {
                // 写日志
                var log = new Log()
                {
                    Action = "Login",
                    Detail = "会员登录:" + u.UserID,
                    CreateDate = DateTime.Now,
                    CreatorLoginName = u.UserID,
                    IpAddress = HttpContext.Current.Request.UserHostAddress

                };
                LogRepository.Add(log);

                // 保存令牌
                UserTokenManager.RemoveTokenWithUserID(u.UserID);
                var userToken = new UserToken
                {
                    UserID = u.UserID,
                    Token = Utility.Encrypt(string.Format("{0}{1}", Guid.NewGuid().ToString("D"), DateTime.Now.Ticks)),
                    Timeout = DateTime.Now.AddHours(8)
                };
                UserTokenManager.AddToken(userToken);

                resultData.Data = new
                {
                    user_id = userToken.UserID,
                    token = userToken.Token,
                    timeout = userToken.Timeout.ToString("yyyy/MM/dd HH:mm:ss")
                };
            }

            return resultData;
        }

        /// <summary>
        /// 退出当前账号
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //public ResultData SignOut()
        //{
        //    // 登录log
        //    var logRep = ContainerManager.Resolve<ISysLogRepository>();
        //    var log = new Log()
        //    {
        //        Action = "SignOut",
        //        Detail = "会员退出:" + RISContext.Current.CurrentUserInfo.UserName,
        //        CreateDate = DateTime.Now,
        //        CreatorLoginName = RISContext.Current.CurrentUserInfo.UserName,
        //        IpAddress = GetClientIp(this.Request)
        //    };
        //    logRep.Add(log);
        //    //System.Web.Security.FormsAuthentication.SignOut();
        //    UserTokenManager.RemoveToken(this.Token);
        //    return new ResultData(ResultType.Success, "退出成功");
        //}
    }
}
