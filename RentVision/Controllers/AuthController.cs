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
using Twinvision.Piranha.RentVision.Helpers;
using Twinvision.Piranha.RentVision.Controllers;
using RentVision.Models;

namespace RentVision.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;
        private ApiHelper _apiHelper;

        public AuthController(IApi api, IHttpClientFactory clientFactory )
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper( _api, _clientFactory );
        }

        [Route("/auth/isUserSiteReady"), HttpPost]
        public async Task<JsonResult> isUserSiteReadyAsync(string email)
        {
            var userSiteReadyResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.UserSiteReady,
                HttpMethod.Get,
                new Dictionary<string, string>() { { "email", email } }
            );

            var response = await userSiteReadyResponse.Content.ReadAsStringAsync();

            return new JsonResult( new { userSiteReadyResponse.StatusCode, response } );
        }

        [Route("/auth/login"), HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password, bool remember)
        {
            string refererUrl = Request.Headers["Referer"].ToString();

            if (email == null)
            {
                return Redirect(refererUrl);
            }

            email = email.ToLower();

            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            var formErrors = AuthHelper.validateForm(Request.Form, userCulture);

            if (formErrors.Count > 0)
            {
                TempData["Errors"] = formErrors.ToArray();

                return Redirect(refererUrl);
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
            };

            // Check if user credentials match user input

            var userCredentialResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.CheckUserCredentials, HttpMethod.Post, urlParameters, password );
            string userCredentialString = await userCredentialResponse.Content.ReadAsStringAsync();

            TempData["StatusCode"] = userCredentialResponse.StatusCode;
            TempData["StatusMessage"] = userCredentialString;

            if ( !userCredentialResponse.IsSuccessStatusCode )
            {
                // Get localized back-end error message and display it to the visitor
                string localizedBackOfficeMessage = AuthHelper.GetBackOfficeStringLocalized(userCulture, userCredentialString);

                if (localizedBackOfficeMessage != null)
                {
                    TempData["StatusMessage"] = localizedBackOfficeMessage;
                }

                return Redirect(refererUrl);
            }

            // Fetch customer subdomain and redirect
            var subdomainResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.UserSubDomain, HttpMethod.Get, urlParameters);
            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();

            return Redirect($"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}");
        }

        [Route("/auth/register"), HttpPost]
        public async Task<IActionResult> RegisterAsync( string userPlan, string payInterval, string email, string subdomain, string businessUnitName, string password, string confirmPassword, bool tos )
        {
            string refererUrl = Request.Headers["Referer"].ToString();

            if ( email == null || subdomain == null )
            {
                return Redirect(refererUrl);
            }

            email = email.ToLower();
            subdomain = subdomain.ToLower();

            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            var formErrors = AuthHelper.validateForm(Request.Form, userCulture);

            if ( formErrors.Count > 0 )
            {
                TempData["Errors"] = formErrors.ToArray();

                return Redirect(refererUrl);
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "userPlan", "Free" },
                { "subDomainName", subdomain },
                { "businessUnitName", businessUnitName }
            };

            // Attempt to create an account if everything is ok

            var response = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.CreateAccount, HttpMethod.Post, urlParameters, password );
            string userCredentialString = await response.Content.ReadAsStringAsync();

            TempData["StatusCode"] = response.StatusCode;
            TempData["StatusMessage"] = userCredentialString;

            if ( !response.IsSuccessStatusCode )
            {
                // Get localized back-end error message and display it to the visitor
                string localizedBackOfficeMessage = AuthHelper.GetBackOfficeStringLocalized(userCulture, userCredentialString);

                if ( localizedBackOfficeMessage != null )
                {
                    TempData["StatusMessage"] = localizedBackOfficeMessage;
                }

                return Redirect(refererUrl);
            }


            // Handle Mollie

            // Always create a customer and return its ID, incase they want to upgrade later on...
            var customerCreationResponse = await CustomerController.CreateCustomerAsync(email, businessUnitName);

            if ( userPlan.ToLower() != "free" )
            {
                int interval = Convert.ToInt32(payInterval);
                var userPlanResponse = await _apiHelper.GetUserPlansAsync();
                UserPlan plan = userPlanResponse.Find(m => m.Name.IndexOf(userPlan) != -1 && m.payInterval == interval);

                var checkoutUrl = await new CustomerController().CreatePaymentRequest(plan);

                if ( checkoutUrl != null )
                {
                    return Redirect(checkoutUrl);
                }
            }

            //return DisplaySubDomainSetup(email, password);
            return new JsonResult(HttpStatusCode.OK);
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
            var getUserKeyParameters = new Dictionary<string, string>() {
                    { "email", email }
                };

            var subdomainResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.UserSubDomain,
                HttpMethod.Get,
                getUserKeyParameters
            );

            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();
            var redirectUrl = $"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}";

            // Add subDomainName to the list of parameters because we need it in the next call
            getUserKeyParameters.Add("subDomainName", subdomain);

            var userKeyResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.GetLoginKey,
                HttpMethod.Post,
                getUserKeyParameters,
                (string)TempData["password"]
            );

            string userKey = await userKeyResponse.Content.ReadAsStringAsync();
            string realRedirectUrl = redirectUrl + "/externLogin?externLoginKey=" + userKey;

            return new JsonResult( new { statusCode = HttpStatusCode.OK, realRedirectUrl } );
        }
    }
}