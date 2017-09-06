using Conch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Conch
{
    public class UserToken
    {
        public string UserID { get; set; }
        public string Token { get; set; }
        public DateTime Timeout { get; set; }
        public int GroupID { get; set; }
    }

    public interface IUserTokenRepository
    {
        IEnumerable<UserToken> GetAll();
        UserToken FindByToken(string token);
        void Add(UserToken ut);
        void Remove(UserToken ut);
        void Update(UserToken ut);
    }

    public class UserTokenManager
    {
        private const string TOKENNAME = "PASSPORT.TOKEN";

        static UserTokenManager()
        {
        }
        /// <summary>
        /// 初始化缓存
        /// </summary>
        private static List<UserToken> InitCache()
        {
            if (HttpRuntime.Cache[TOKENNAME] == null)
            {
                var tokens = new List<UserToken>();
                // cache 的过期时间， 令牌过期时间 *2
                HttpRuntime.Cache.Insert(TOKENNAME, tokens, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(7 * 2));
            }
            var ts = (List<UserToken>)HttpRuntime.Cache[TOKENNAME];
            return ts;
        }


        //public static int GetUId(string token)
        //{
        //    var tokens = InitCache();
        //    var result = 0;
        //    if (tokens.Count > 0)
        //    {
        //        var id = tokens.Where(c => c.Token == token).Select(c => c.UId).FirstOrDefault();
        //        if (id != null)
        //            result = id.Value;
        //    }
        //    return result;
        //}


        //public static string GetPermission(string token)
        //{
        //    var tokens = InitCache();
        //    if (tokens.Count == 0)
        //        return "NoAuthorize";
        //    else
        //        return tokens.Where(c => c.Token == token).Select(c => c.Permission).FirstOrDefault();
        //}

        //public static string GetUserType(string token)
        //{
        //    var tokens = InitCache();
        //    if (tokens.Count == 0)
        //        return "";
        //    else
        //        return tokens.Where(c => c.Token == token).Select(c => c.UserType).FirstOrDefault();
        //}

        /// <summary>
        /// 判断令牌是否存在
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsExistToken(string token)
        {
            var tokens = InitCache();
            if (tokens.Count == 0) return false;
            else
            {
                var t = tokens.Where(c => c.Token == token).FirstOrDefault();
                if (t == null)
                    return false;
                else if (t.Timeout < DateTime.Now)
                {
                    RemoveToken(t);
                    return false;
                }
                else
                {
                    // 小于8小时 更新过期时间
                    if ((t.Timeout - DateTime.Now).TotalMinutes < 1 * 60 - 1)
                    {
                        t.Timeout = DateTime.Now.AddHours(8);
                        UpdateToken(t);
                    }
                    return true;
                }

            }
        }
        public static bool FindToken(string token, out UserToken ut)
        {
            ut = null;
            var tokens = InitCache();
            if (tokens.Count == 0) return false;
            else
            {
                var t = tokens.Where(c => c.Token == token).FirstOrDefault();
                if (t == null)
                    return false;
                else if (t.Timeout < DateTime.Now)
                {
                    RemoveToken(t);
                    return false;
                }
                else
                {
                    ut = t;
                    return true;
                }

            }
        }

        /// <summary>
        /// 添加令牌， 没有则添加，有则更新
        /// </summary>
        /// <param name="token"></param>
        public static void AddToken(UserToken token)
        {
            var tokens = InitCache();
            // 不存在  怎增加
            if (!IsExistToken(token.Token))
            {
                tokens.Add(token);
            }
            else  // 有则更新
            {
                UpdateToken(token);
            }
        }

        public static bool UpdateToken(UserToken token)
        {
            var tokens = InitCache();
            if (tokens.Count == 0) return false;
            else
            {
                var t = tokens.Where(c => c.Token == token.Token).FirstOrDefault();
                if (t == null)
                    return false;
                t.Timeout = token.Timeout;
                return true;
            }
        }
        /// <summary>
        /// 移除指定令牌
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static void RemoveToken(UserToken token)
        {
            var tokens = InitCache();
            if (tokens.Count == 0) return;
            tokens.Remove(token);
        }

        public static void RemoveToken(string token)
        {
            var tokens = InitCache();
            if (tokens.Count == 0) return;

            var ts = tokens.Where(c => c.Token == token).ToList();
            foreach (var t in ts)
            {
                tokens.Remove(t);
            }
        }
        public static void RemoveTokenWithUserID(string userID)
        {
            var tokens = InitCache();
            if (tokens.Count == 0) return;

            var ts = tokens.Where(c => c.UserID == userID).ToList();
            foreach (var t in ts)
            {
                tokens.Remove(t);
            }
        }
    }
}