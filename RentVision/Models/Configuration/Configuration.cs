﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentVision.Models.Configuration
{
    public class Configuration
    {
        public class BackOffice
        {
            public const string Protocol            = "https";
#if DEBUG
            public const string HostName            = "rentvision_backoffice.eu.ngrok.io/api";
#else
            public const string HostName            = "backoffice.rentvision.eu/api";
#endif
            public const string ApiKeyHeaderName    = "X-Api-Key";
            public const string ApiKey              = "TestKey";
            public static string Url = $"{Protocol}://{HostName}";
        }

        public class Website
        {
            public const string Protocol = "https";
#if DEBUG
            public const string HostName = "rentvision.eu.ngrok.io";
#else
            public const string HostName = "rentvision.eu";
#endif
            public static string Url = $"{Protocol}://{HostName}";
        }

        public class ApiCalls
        {
            // RentVisionAccount
            public const string CreateAccount           = "RentVisionAccount/CreateAccount";
            public const string LoginUserRentVisionApi  = "RentVisionAccount/LoginUserRentVisionApi";
            public const string UserPlans               = "RentVision/UserPlans";
            public const string SetPlan                 = "RentVisionAccount/SetPlan";
            public const string SingleUserPlan          = "RentVisionAccount/SingleUserPlan";
            public const string UserSiteReady           = "RentVisionAccount/UserSiteReady";
            public const string UserSubDomain           = "RentVisionAccount/UserSubDomain";
            public const string DeleteAccount           = "RentVisionAccount/DeleteAccount";
            public const string GetRentVisionLoginKey   = "RentVisionAccount/GetRentVisionLoginKey";
            public const string GetMollieId             = "RentVisionAccount/GetMollieId";
            public const string SetMollieId             = "RentVisionAccount/SetMollieId";
            public const string GetEmail                = "RentVisionAccount/GetEmail";
            public const string GetPayment              = "RentVisionAccount/GetPayment";
            // VerificationCode
            public const string CreateVerificationCode      = "VerificationCode/Create";
            public const string SetVerificationCodeVerified = "VerificationCode/Verified";
            public const string GetVerificationCodeStatus   = "VerificationCode/Verified";
            // Misc
            public const string PaymentWebhook          = "Mollie/PaymentWebhook";
            public const string SubscriptionWebhook     = "Mollie/SubscriptionWebhook";
            public const string GetTransactionStatus    = "Mollie/GetTransactionStatus";
            public const string KillAllSites            = "RentVision/KillAllSites";
        }
    }
}
