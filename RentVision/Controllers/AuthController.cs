using RentVision.Models;
using RentVision.Models.Regions;
using Microsoft.AspNetCore.Mvc;
using Piranha;
using Piranha.Extend.Blocks;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using RentVision.Models.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using Piranha.AspNetCore.Services;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Linq;

namespace RentVision.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IApi api, IHttpClientFactory clientFactory )
        {
            _api = api;
            _clientFactory = clientFactory;
        }

        [Route("/auth/isUserSiteReady"), HttpPost]
        public async Task<JsonResult> isUserSiteReadyAsync(string email)
        {
            var userSiteReadyResponse = await SendApiCallAsync(
                Configuration.ApiCalls.UserSiteReady,
                new Dictionary<string, string>() { { "email", email.ToLower() } },
                HttpMethod.Get
            );

            var response = await userSiteReadyResponse.Content.ReadAsStringAsync();

            return new JsonResult( new { userSiteReadyResponse.StatusCode, response } );
        }

        [Route("/auth/login"), HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password, bool remember)
        {
            var formErrors = validateForm(Request.Form);

            if (formErrors.Count > 0)
            {
                TempData["Errors"] = formErrors.ToArray();

                return RedirectToAction("login", "cms");
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email.ToLower() }
            };

            var userCredentialResponse = await SendApiCallAsync(Configuration.ApiCalls.CheckUserCredentials, urlParameters, HttpMethod.Post, password );

            TempData["StatusCode"] = userCredentialResponse.StatusCode;
            TempData["StatusMessage"] = await userCredentialResponse.Content.ReadAsStringAsync();

            if ( !userCredentialResponse.IsSuccessStatusCode )
            {
                return RedirectToAction( "login", "cms" );
            }

            // Subdomein ophalen en doorsturen
            var subdomainResponse = await SendApiCallAsync(Configuration.ApiCalls.UserSubDomain, urlParameters, HttpMethod.Get);
            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();

            return Redirect($"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}");
        }

        [Route("/auth/register"), HttpPost]
        public async Task<IActionResult> RegisterAsync( string email, string subdomain, string businessUnitName, string password, string confirmPassword, bool tos )
        {
            var formErrors = validateForm(Request.Form);

            if ( formErrors.Count > 0 )
            {
                TempData["Errors"] = formErrors.ToArray();

                return RedirectToAction("register", "cms");
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email.ToLower() },
                { "userPlan", "Free" },
                { "subDomainName", subdomain.ToLower() },
                { "businessUnitName", businessUnitName }
            };

             var response = await SendApiCallAsync(Configuration.ApiCalls.CreateAccount, urlParameters, HttpMethod.Post, password );

            TempData["StatusCode"] = response.StatusCode;
            TempData["StatusMessage"] = await response.Content.ReadAsStringAsync();

            if ( !response.IsSuccessStatusCode )
            {
                return RedirectToAction("register", "cms");
            }

            return DisplaySubDomainSetup(email, password);
        }

        public List<string> validateForm(IFormCollection form)
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

            foreach ( var formItem in form)
            {
                // Check textFields
                if ( textFields.ContainsKey(formItem.Key) )
                {
                    if ( string.IsNullOrWhiteSpace( Request.Form[formItem.Key] ))
                    {
                        errors.Add( textFields[formItem.Key] + " is required.");
                    }
                }
            }

            // Register page checks
            if (form.Keys.Contains("confirmPassword") )
            {
                List<string> registerErrors = checkRegisterPageFields(form);

                errors.AddRange(registerErrors);
            }

            return errors;
        }

        public List<string> checkRegisterPageFields(IFormCollection form)
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

        public bool VerifyEmailAddress(string email)
        {
            var pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var match = Regex.Match(email, pattern);

            if (match.Success)
            {
                return true;
            }

            return false;
        }

        public IActionResult DisplaySubDomainSetup(string email, string password)
        {
            TempData["Email"] = email;
            TempData["Password"] = password;

            return RedirectToAction("setup", "cms");
        }

        [Route("/auth/getUserKey"), HttpPost]
        public async Task<JsonResult> GetUserKey( string email )
        {
            var subdomainResponse = await SendApiCallAsync(
                Configuration.ApiCalls.UserSubDomain,
                new Dictionary<string, string>() { { "email", email } },
                HttpMethod.Get
            );

            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();
            var redirectUrl = $"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}";

            // TODO: API CALL GetLoginKey AANROEPEN (subDomainName, email, password)
            var userKeyResponse = await SendApiCallAsync("GetLoginKey",
                new Dictionary<string, string>() {
                    { "subDomainName", subdomain },
                    { "email", email }
                },
                HttpMethod.Post,
                (string)TempData["password"]
            );

            string userKey = await userKeyResponse.Content.ReadAsStringAsync();
            string realRedirectUrl = redirectUrl + "/login?externLoginKey=" + userKey;

            return new JsonResult( new { statusCode = HttpStatusCode.OK, realRedirectUrl } );
        }

        /// <summary>
        /// Sends an async api call to the rentvision backoffice api and returns an HttpResponseMessage object
        /// </summary>
        /// <param name="callType">The type of call that should be called (Mapped to Configuration.BackOffice or ApiCalls)</param>
        /// <param name="data">A Dictionary containing parameter data</param>
        /// <param name="callMethod">The HttpMethod that should be used to send data</param>
        /// <returns>An HttpResponseMessage containing response data from the API</returns>
        public async Task<HttpResponseMessage> SendApiCallAsync(string callType, Dictionary<string, string> data, HttpMethod callMethod, string password = null )
        {
            var query = QueryHelpers.AddQueryString(
                $"{Configuration.BackOffice.Protocol}://{Configuration.BackOffice.HostName}/{callType}",
                data
            );

            var request = new HttpRequestMessage
            {
                Method = callMethod,
                RequestUri = new Uri(query)
            };

            if ( password != null )
            {
                request.Headers.Add("X-Password", password);
            }

            var client = _clientFactory.CreateClient("RentVisionApi");

            var response = await client.SendAsync(request);

            return response;
        }
    }
}
