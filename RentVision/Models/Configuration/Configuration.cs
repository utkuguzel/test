using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentVision.Models.Configuration
{
    public class Configuration
    {
        public class BackOffice
        {
            public static string Protocol { get; set; }
            public static string HostName { get; set; }
            public static string ApiKeyHeaderName { get; set; }
            public static string ApiKey { get; set; }
        }

        public class ApiCalls
        {
            public static string CreateAccount { get; set; }
            public static string CheckUserCredentials { get; set; }
            public static string UserPlans { get; set; }
            public static string SetPlan { get; set; }
            public static string SingleUserPlan { get; set; }
            public static string UserSiteReady { get; set; }
            public static string UserSubDomain { get; set; }
            public static string DeleteAccount { get; set; }
        }
    }
}
