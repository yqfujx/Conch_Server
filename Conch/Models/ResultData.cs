using System;

namespace Conch.Models
{
    public enum ResultType : int
    {
        UnknowError = -1,

        Success = 200,
        Unauthorized = 401,
        NotFound = 404,


        DatabaseError = 1001,
        VerifyCodeError = 1002,


        UserNameUnexists = 2001,
        UserNameOrPasswordError = 2002,
        BadParameter = 2003
    }
    public class EnumExtension
    {
        public static string GetEnumDescription(ResultType result)
        {
            string desc = "";

            switch (result)
            {
                case ResultType.UnknowError:
                    desc = "未知错误";
                    break;
                case ResultType.Success:
                    desc = "成功";
                    break;
                case ResultType.Unauthorized:
                    desc = "权限不足";
                    break;
                case ResultType.NotFound:
                    desc = "资源不存在";
                    break;
                case ResultType.VerifyCodeError:
                    desc = "验证码错误";
                    break;
                case ResultType.DatabaseError:
                    desc = "数据库错误";
                    break;
                case ResultType.UserNameUnexists:
                    desc = "用户名不存在";
                    break;
                case ResultType.UserNameOrPasswordError:
                    desc = "用户名或密码错误";
                    break;
            }

            return desc;
        }
    }

    public class ResultData
    {
        public int Result { get; set; }
        public string Desc { get; set; }
        public object Data { get; set; }

        public ResultData(){ }
        public ResultData(int result, string desc)
        {
            this.Result = result;
            this.Desc = desc;
        }
        public ResultData(ResultType result, string desc)
        {
            this.Result = Convert.ToInt32(result);
            this.Desc = desc;
        }

        public ResultData(ResultType result, string desc, object data)
        {
            this.Result = Convert.ToInt32(result);
            this.Desc = desc;
            this.Data = data;
        }
    }
}