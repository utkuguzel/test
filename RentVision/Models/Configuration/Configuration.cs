using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RentVision.Models.Configuration
{
    public class Configuration
    {
        private static bool useNGrok = false;

        public class BackOffice
        {
            public const string Protocol = "https";
            private static string hostName = null;
            public static string HostName
            {
                get
                {
                    if (hostName == null)
                    {
                        if (Debugger.IsAttached && useNGrok)
                        {
                            hostName = "rentvision_backoffice.eu.ngrok.io/api";
                        }
                        else
                        {
                            hostName = "backoffice.rentvision.eu/api";
                        }
                    }
                    return hostName;
                }
            }

            public const string ApiKeyHeaderName = "X-Api-Key";
            public const string ApiKey = "TestKey";
            public static string Url = $"{Protocol}://{HostName}";
        }

        public class Website
        {
            public const string Protocol = "https";
            private static string hostName = null;
            public static string HostName
            {
                get
                {
                    if (hostName == null)
                    {
                        if (Debugger.IsAttached && useNGrok)
                        {
                            hostName = "rentvision.eu.ngrok.io";
                        }
                        else
                        {
                            hostName = "rentvision.eu";
                        }
                    }
                    return hostName;
                }
            }
            public static string Url = $"{Protocol}://{HostName}";
        }

        public enum ApiGroup
        {
            RentVisionAccount,
            RentVision,
            VerificationCode,
            Mollie,
            Plans
        }

        public class ApiCall
        {
            public ApiGroup ApiCategory { get; set; }
            public string Url { get; set; }
            public HttpMethod Method { get; set; }

            public ApiCall(ApiGroup apiCategory, string url, HttpMethod method) =>
                (ApiCategory, Url, Method) = (apiCategory, url, method);

            public string FullUrl()
            {
                return $"{Enum.GetName(typeof(ApiGroup), ApiCategory)}/{Url}";
            }
        }

        public class ApiCalls
        {
            // RentVisionAccount
            public static ApiCall CreateAccount = new ApiCall(ApiGroup.RentVisionAccount, "CreateAccount", HttpMethod.Post);
            public static ApiCall LoginUserRentVisionApi = new ApiCall(ApiGroup.RentVisionAccount, "LoginUserRentVisionApi", HttpMethod.Post);
            public static ApiCall SetPlan = new ApiCall(ApiGroup.RentVisionAccount, "SetPlan", HttpMethod.Post);
            public static ApiCall SingleUserPlan = new ApiCall(ApiGroup.RentVisionAccount, "SingleUserPlan", HttpMethod.Get);
            public static ApiCall UserSiteReady = new ApiCall(ApiGroup.RentVisionAccount, "UserSiteReady", HttpMethod.Get);
            public static ApiCall UserSubDomain = new ApiCall(ApiGroup.RentVisionAccount, "UserSubDomain", HttpMethod.Get);
            public static ApiCall DeleteAccount = new ApiCall(ApiGroup.RentVisionAccount, "DeleteAccount", HttpMethod.Post);
            public static ApiCall GetRentVisionLoginKey = new ApiCall(ApiGroup.RentVisionAccount, "GetRentVisionLoginKey", HttpMethod.Post);
            public static ApiCall GetMollieId = new ApiCall(ApiGroup.RentVisionAccount, "GetMollieId", HttpMethod.Get);
            public static ApiCall SetMollieId = new ApiCall(ApiGroup.RentVisionAccount, "SetMollieId", HttpMethod.Post);
            public static ApiCall GetEmail = new ApiCall(ApiGroup.RentVisionAccount, "GetEmail", HttpMethod.Get);
            public static ApiCall GetPayments = new ApiCall(ApiGroup.RentVisionAccount, "GetPayments", HttpMethod.Get);

            // RentVision
            public static ApiCall UserPlans = new ApiCall(ApiGroup.RentVision, "UserPlans", HttpMethod.Get);
            public static ApiCall KillAllTestSites = new ApiCall(ApiGroup.RentVision, "KillAllTestSites", HttpMethod.Post);

            // VerificationCode
            public static ApiCall CreateVerificationCode = new ApiCall(ApiGroup.VerificationCode, "Create", HttpMethod.Post);
            public static ApiCall SetVerificationCodeVerified = new ApiCall(ApiGroup.VerificationCode, "Verified", HttpMethod.Post);
            public static ApiCall GetVerificationCodeStatus = new ApiCall(ApiGroup.VerificationCode, "Verified", HttpMethod.Get);

            // Mollie
            public static ApiCall PaymentWebhook = new ApiCall(ApiGroup.Mollie, "PaymentWebhook", HttpMethod.Post);
            public static ApiCall GetTransactionStatus = new ApiCall(ApiGroup.Mollie, "GetTransactionStatus", HttpMethod.Get);
            public static ApiCall MollieGetCustomers = new ApiCall(ApiGroup.Mollie, "Customers", HttpMethod.Get);
            public static ApiCall MollieGetPayments = new ApiCall(ApiGroup.Mollie, "Payments", HttpMethod.Get);
            public static ApiCall MollieCreateCustomer = new ApiCall(ApiGroup.Mollie, "Customer", HttpMethod.Post);
            public static ApiCall MollieCreatePayment = new ApiCall(ApiGroup.Mollie, "Payment", HttpMethod.Post);
            public static ApiCall MollieCreateSubscription = new ApiCall(ApiGroup.Mollie, "Subscription", HttpMethod.Post);

            // Plans
            public static ApiCall GetPlans = new ApiCall(ApiGroup.RentVision, "UserPlans", HttpMethod.Get);
            public static ApiCall GetPlanFeatures = new ApiCall(ApiGroup.RentVision, "PlanFeatures", HttpMethod.Get);
        }
    }
}
