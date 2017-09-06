using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Configuration;
using PushSharp.Apple;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace Conch.App_Start
{
    public interface IPushNotificationRepository
    {
        ApnsNotification[] All();
    }
    public class DefaultPushNotificationRepository: IPushNotificationRepository
    {
        public ApnsNotification[] All()
        {
            List<ApnsNotification> array = new List<ApnsNotification>();
            using (SqlConnection con = new SqlConnection(WebApiConfig.ConnectionString))
            {
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "Pr_AppPush";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var token = reader.GetString(0);
                            var deviceType = reader.GetValue(1);
                            var badge = reader.GetInt32(2);
                            if (token != null && badge > 0)
                            {
                                var plString = "{\"aps\":{\"alert\":\"您有" + badge + "条记录待审批\",\"badge\":" + badge + ", \"sound\":\"chime.aiff\"}}";
                                var payload = JObject.Parse(plString);

                                array.Add(new ApnsNotification(token, payload));
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return array.ToArray();
        }
    }
    public class PushTaskSchedule
    {
        public IPushNotificationRepository Repository { get; set; }

        private static Object lockGuard = new object();

        private ApnsConfiguration apnsConfig;
        private Timer timer;

        public PushTaskSchedule(IPushNotificationRepository repository = null)
        {
            if (repository == null)
            {
                repository = new DefaultPushNotificationRepository();
            }
            this.Repository = repository;
        }
        public void Start()
        {
            var settings = ConfigurationManager.AppSettings;

            bool isDebug = false;
            var value = settings["isDebug"];
            if (value != null)
            {
                isDebug = Convert.ToBoolean(value);
            }

            string certFile = "aps.p12";
            string certPwd = "aps@zonjli";
            value = settings["certFile"];
            if (value != null)
            {
                certFile = value;
            }
            value = settings["certPwd"];
            if (value != null)
            {
                certPwd = value;
            }

            ApnsConfiguration.ApnsServerEnvironment env = isDebug ? ApnsConfiguration.ApnsServerEnvironment.Sandbox : ApnsConfiguration.ApnsServerEnvironment.Production;
            this.apnsConfig = new ApnsConfiguration(env, certFile, certPwd);


            double interval = 1000 * 60 * 5;
            value = settings["pushIntervalMinutes"];
            if (value != null)
            {
                interval = 1000 * 60 * Convert.ToDouble(value);
            }

            this.HandleTimer();
            this.timer = new Timer(interval);
            this.timer.Elapsed += (sender, e) => this.HandleTimer();
            this.timer.AutoReset = true;
            this.timer.Start();
        }
        protected void HandleTimer()
        {
            lock (lockGuard)
            {
                var array = this.Repository.All();
                if (array.Length <= 0)
                {
                    return;
                }

                List<ApnsNotification> successArray = new List<ApnsNotification>();

                var apnsBroker = new ApnsServiceBroker(this.apnsConfig);
                // Wire up events
                apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                    aggregateEx.Handle(ex => {

                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException			
                            Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                apnsBroker.OnNotificationSucceeded += (notification) => {
                    Console.WriteLine("Apple Notification Sent!");
                    successArray.Add(notification);
                };

                // Start the broker
                apnsBroker.Start();

                foreach (var noti in array)
                {
                    apnsBroker.QueueNotification(noti);
                }

                // Stop the broker, wait for it to finish   
                // This isn't done after every message, but after you're
                // done with the broker
                apnsBroker.Stop();

            } // end of lock

            return;
        }
    }
}