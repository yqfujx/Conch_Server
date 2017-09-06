using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Conch.Common;
using Conch.Models;
using System.Data;

namespace Conch
{
    public class UserManager
    {
        public static ResultType Login(LoginUser u, out User user)
        {
            user = null;
            var pwd = SymmetricMethod.EncryptToSelf(u.Password);
            var sessionToken = Utility.Encrypt(string.Format("{0}{1}", Guid.NewGuid().ToString("D"), DateTime.Now.Ticks));
            var timeout = DateTime.Now.AddHours(8);

            var ret = ResultType.Success;
            var conn = new SqlConnection(WebApiConfig.ConnectionString);
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[Pr_AppLogin]";
                var retVal = cmd.Parameters.Add("ret_val", SqlDbType.Int);
                retVal.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(new SqlParameter("@user_id", u.UserID));
                cmd.Parameters.Add(new SqlParameter("@password", pwd));
                cmd.Parameters.Add(new SqlParameter("client", u.Client));
                cmd.Parameters.Add(new SqlParameter("app_version", u.AppVersion));
                cmd.Parameters.Add(new SqlParameter("push_token", u.PushToken));

                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    user = new User
                    {
                        UserID = reader.GetString(0),
                        GroupID = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        Client = u.Client,
                        AppVersion = u.AppVersion,
                        PushToken = u.PushToken
                    };
                }
                conn.Close();

                switch (retVal.Value)
                {
                    case 0:
                        ret = ResultType.Success;
                        break;
                    case -1:
                        ret = ResultType.UserNameUnexists;
                        break;
                    case -2:
                        ret = ResultType.UserNameOrPasswordError;
                        break;
                    case -3:
                    default:
                        ret = ResultType.DatabaseError;
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ret = ResultType.DatabaseError;
            }
            finally
            {
                conn.Close();
            }

            return ret;

        }
    }
}
