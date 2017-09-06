using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Conch.Models
{
    public class Log
    {
        public string Action { get; set; }
        public string Detail { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatorLoginName { get; set; }
        public string IpAddress { get; set; }
    }
    public class LogRepository
    {
        public static void Add(Log log)
        {

        }
    }

}