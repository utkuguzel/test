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
        public static List<string> validateForm(IFormCollection form)
        {
            Dictionary<string, string> textFields = new Dictionary<string, string>()
            {
                { "email", "E-mailaddress" },
                { "subdomain", "Subdomain" },
                { "businessUnitName", "Business name" },
                { "password", "Password" },
                { "confirmPassword", "Confirm password" }
            };

            List<string> errors = new List<string>();

            foreach (var formItem in form)
            {
                // Check textFields
                if (textFields.ContainsKey(formItem.Key))
                {
                    if (string.IsNullOrWhiteSpace(form[formItem.Key]))
                    {
                        errors.Add(textFields[formItem.Key] + " is required.");
                    }
                }
            }

            // Register page checks
            if (form.Keys.Contains("confirmPassword"))
            {
                List<string> registerErrors = checkRegisterPageFields(form);

                errors.AddRange(registerErrors);
            }

            return errors;
        }

        public static List<string> checkRegisterPageFields(IFormCollection form)
        {
            List<string> errors = new List<string>();

            if (!form.Keys.Contains("tos"))
            {
                errors.Add("You must agree with our Terms of Service before continuing.");
            }

            if (form["confirmPassword"] != form["password"])
            {
                errors.Add("Passwords do not match.");
            }

            // Check mail address format
            bool isValidEmailAddress = VerifyEmailAddress(form["email"]);

            if (!isValidEmailAddress)
            {
                errors.Add("Invalid e-mailaddress specified.");
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
    }
}
