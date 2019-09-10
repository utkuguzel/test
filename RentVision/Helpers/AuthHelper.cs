using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RentVision.Helpers
{
    public class AuthHelper
    {
        private static Dictionary<string, string> backOfficeMessages = new Dictionary<string, string>()
        {
            { "ERROR_ACCOUNT_NOT_FOUND", "account was not found" },
            { "ERROR_EMAIL_EXISTING", "email already exists" },
            { "ERROR_REGISTER_PASSWORD_STRENGTH", "password was not strong enough" },
            { "ERROR_LOGIN_PASSWORd_INCORRECT", "password did not match" },
            { "ERROR_LOGIN_EMAIL_INCORRECT", "email not found" },
        };


        public static List<string> validateForm(IFormCollection form, string userCulture)
        {
            var localizedStringSection = Startup.Config.GetSection("LocalizedStrings");

            Dictionary<string, string> textFields = new Dictionary<string, string>()
            {
                { "email", localizedStringSection[$"{userCulture}:FIELD_EMAIL"] },
                { "subdomain", localizedStringSection[$"{userCulture}:FIELD_SUBDOMAIN"] },
                { "businessUnitName", localizedStringSection[$"{userCulture}:FIELD_BUSINESS_NAME"] },
                { "password", localizedStringSection[$"{userCulture}:FIELD_PASSWORD"] },
                { "confirmPassword", localizedStringSection[$"{userCulture}:FIELD_PASSWORD_CONFIRM"] }
            };

            List<string> errors = new List<string>();

            foreach (var formItem in form)
            {
                // Check textFields
                if (textFields.ContainsKey(formItem.Key))
                {
                    if (string.IsNullOrWhiteSpace(form[formItem.Key]))
                    {
                        errors.Add($"{textFields[formItem.Key]} {localizedStringSection[$"{userCulture}:ERROR_FIELD_REQUIRED"]}");
                    }
                }
            }

            // Register page checks
            if (form.Keys.Contains("confirmPassword"))
            {
                List<string> registerErrors = checkRegisterPageFields(form, userCulture);

                errors.AddRange(registerErrors);
            }

            return errors;
        }

        public static List<string> checkRegisterPageFields(IFormCollection form, string userCulture)
        {
            List<string> errors = new List<string>();
            var localizedStringSection = Startup.Config.GetSection("LocalizedStrings");

            if (!form.Keys.Contains("tos"))
            {
                errors.Add(localizedStringSection[$"{userCulture}:ERROR_TERMS_OF_SERVICE"]);
            }

            if (form["confirmPassword"] != form["password"])
            {
                errors.Add(localizedStringSection[$"{userCulture}:ERROR_REGISTER_PASSWORD_MATCH"]);
            }

            // Check mail address format
            bool isValidEmailAddress = VerifyEmailAddress(form["email"]);

            if (!isValidEmailAddress)
            {
                errors.Add(localizedStringSection[$"{userCulture}:ERROR_REGISTER_INVALID_EMAIL"]);
            }

            return errors;
        }

        public static bool VerifyEmailAddress(string email)
        {
            var pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var match = Regex.Match(email, pattern);

            if (match.Success)
            {
                return true;
            }

            return false;
        }

        public static string GetBackOfficeStringLocalized(string userCulture, string backOfficeMessage)
        {
            var localizedStringSection = Startup.Config.GetSection("LocalizedStrings");

            foreach (KeyValuePair<string, string> messageDict in backOfficeMessages)
            {
                if (backOfficeMessage.ToLower().Replace(".", "") == messageDict.Value)
                {
                    return Startup.Config.GetSection("LocalizedStrings")[$"{userCulture}:{messageDict.Key}"];
                }
            }

            return null;
        }
    }
}
