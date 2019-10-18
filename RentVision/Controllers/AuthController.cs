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
using Microsoft.AspNetCore.Http;
using reCAPTCHA.AspNetCore;
using Newtonsoft.Json;

namespace RentVision.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;
        private ApiHelper _apiHelper;
        private IRecaptchaService _recaptcha;

        public AuthController(IApi api, IHttpClientFactory clientFactory, IRecaptchaService recaptcha )
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper( _api, _clientFactory );
            _recaptcha = recaptcha;
        }

        [Route("/auth/isUserSiteReady"), HttpPost]
        public async Task<JsonResult> isUserSiteReadyAsync(string email)
        {
            var userSiteReadyResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.UserSiteReady,
                HttpMethod.Get,
                new Dictionary<string, string>() { { "email", email } },
                context: HttpContext
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
            var userCredentialResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.LoginUserRentVisionApi, HttpMethod.Post, urlParameters, password, HttpContext);
            string userCredentialString = await userCredentialResponse.Content.ReadAsStringAsync();
            if (Guid.TryParse(userCredentialString, out Guid apiLoginKey))
            {
                var expirationOffset = new DateTimeOffset().ToOffset(TimeSpan.FromMinutes(20));
                HttpContext.Session.SetString("ApiLoginKey", userCredentialString);
                Response.Cookies.Append("ApiLoginKey", userCredentialString, options: new CookieOptions { Expires = expirationOffset });
            }

            TempData["StatusCode"] = userCredentialResponse.StatusCode;
            TempData["StatusMessage"] = "";

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
            var subdomainResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.UserSubDomain, HttpMethod.Get, urlParameters, context: HttpContext);
            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();

            return Redirect($"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}");
        }

        [Route("/auth/register"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAsync( string email, string subdomain, string businessUnitName, string password, string confirmPassword, bool tos )
        {
            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            string refererUrl = Request.Headers["Referer"].ToString();
            // Get userPlan
            string userPlan = HttpContext.Session.GetString("UserPlan") ?? "Free";
            string payInterval = HttpContext.Session.GetString("PayInterval") ?? "2";
            int interval = Convert.ToInt32(payInterval);
            var userPlanResponse = await _apiHelper.GetUserPlansAsync();
            UserPlan plan = userPlanResponse.Find(m => m.Name.IndexOf(userPlan) != -1 && m.PayInterval == interval);

            var recaptchaResponse = await _recaptcha.Validate(Request.Form["g-recaptcha-response"]);
            if ( !recaptchaResponse.success )
            {
                TempData["StatusMessage"] = AuthHelper.GetBackOfficeStringLocalized(userCulture, "captcha incorrect");
                return Redirect(refererUrl);
            }

            if ( email == null || subdomain == null )
            {
                return Redirect(refererUrl);
            }

            email = email.ToLower();
            subdomain = subdomain.ToLower();

            var formErrors = AuthHelper.validateForm(Request.Form, userCulture);
            if ( formErrors.Count > 0 )
            {
                TempData["Errors"] = formErrors.ToArray();
                return Redirect(refererUrl);
            }

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "userPlanName", plan.Name },
                { "subDomainName", subdomain },
                { "businessUnitName", businessUnitName }
            };

            // Attempt to create an account if everything is ok
            var response = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.CreateAccount, HttpMethod.Post, urlParameters, password);
            string userCredentialString = await response.Content.ReadAsStringAsync();
            if ( Guid.TryParse(userCredentialString, out Guid apiLoginKey) )
            {
                var expirationOffset = new DateTimeOffset().ToOffset(TimeSpan.FromMinutes(20));
                HttpContext.Session.SetString("ApiLoginKey", userCredentialString);
                CookieHelper.SetCookie("ApiLoginKey",
                    userCredentialString,
                    HttpContext, options: new CookieOptions { Expires = expirationOffset }
                );
            }

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

            // Create verification code
            var verificationCodeParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "culture", userCulture }
            };
            var verificationCodeResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.CreateVerificationCode,
                HttpMethod.Post,
                verificationCodeParameters,
                context: HttpContext
            );
            if ( !verificationCodeResponse.IsSuccessStatusCode )
            {
                TempData["StatusMessage"] = await verificationCodeResponse.Content.ReadAsStringAsync();
                return Redirect(refererUrl);
            }

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
                getUserKeyParameters,
                context: HttpContext
            );

            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();
            var redirectUrl = $"{Configuration.BackOffice.Protocol}://{subdomain}.{Configuration.BackOffice.Domain}";

            // Add subDomainName to the list of parameters because we need it in the next call
            getUserKeyParameters.Add("subDomainName", subdomain);

            var userKeyResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.GetRentVisionLoginKey,
                HttpMethod.Post,
                getUserKeyParameters,
                context: HttpContext
            );

            string userKey = await userKeyResponse.Content.ReadAsStringAsync();
            string realRedirectUrl = redirectUrl + "/externLogin?externLoginKey=" + userKey;

            return new JsonResult( new { statusCode = HttpStatusCode.OK, realRedirectUrl } );
        }
    }
}