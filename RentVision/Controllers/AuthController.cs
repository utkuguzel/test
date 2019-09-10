using Microsoft.AspNetCore.Mvc;
using Piranha;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using RentVision.Models.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using RentVision.Helpers;

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
                new Dictionary<string, string>() { { "email", email } },
                HttpMethod.Get
            );

            var response = await userSiteReadyResponse.Content.ReadAsStringAsync();

            return new JsonResult( new { userSiteReadyResponse.StatusCode, response } );
        }

        [Route("/auth/login"), HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password, bool remember)
        {
            email = email.ToLower();

            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            var formErrors = AuthHelper.validateForm(Request.Form, userCulture);

            if (formErrors.Count > 0)
            {
                TempData["Errors"] = formErrors.ToArray();

                return RedirectToAction("login", "cms");
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
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
            email = email.ToLower();
            subdomain = subdomain.ToLower();

            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            var formErrors = AuthHelper.validateForm(Request.Form, userCulture);

            if ( formErrors.Count > 0 )
            {
                TempData["Errors"] = formErrors.ToArray();

                return RedirectToAction("register", "cms");
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "userPlan", "Free" },
                { "subDomainName", subdomain },
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
