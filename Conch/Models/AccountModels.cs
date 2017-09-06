using System;

namespace Conch.Models
{
    public class User
    {
        public string UserID { get; internal set; }
        public string Client { get; set; }
        public string AppVersion { get; set; }
        public string PushToken { get; set; }
        public int GroupID { get; internal set; }
    }
    public class LoginUser
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Client { get; set; }
        public string AppVersion { get; set; }
        public string PushToken { get; set; }
    }

}