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
using static RentVision.Models.Configuration.Configuration;

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

        [HttpPost("/auth/isUserSiteReady")]
        public async Task<JsonResult> IsUserSiteReadyAsync(string email)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
            };
            var userSiteReadyRequest = await _apiHelper.SendApiCallAsync(ApiCalls.UserSiteReady, urlParameters, context: HttpContext);
            var response = await userSiteReadyRequest.Content.ReadAsStringAsync();
            return new JsonResult( new { userSiteReadyRequest.StatusCode, response } );
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/auth/login")]
        public async Task<IActionResult> LoginAsync(LoginPage model)
        {
            var pageModel = await _api.Pages.GetByIdAsync<LoginPage>(model.Id);
            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);

            if ( string.IsNullOrEmpty(model.Email))
            {
                return View("~/Views/Cms/login.cshtml", pageModel);
            }

            model.Email = model.Email.ToLower();

            // Check if user credentials match user input
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", model.Email }
            };
            var userCredentialResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.LoginUserRentVisionApi,
                urlParameters,
                password: model.Password,
                context: HttpContext
            );
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
                string localizedBackOfficeMessage = AuthHelper.GetBackOfficeStringLocalized(userCulture, userCredentialString);
                if (localizedBackOfficeMessage != null)
                {
                    TempData["StatusMessage"] = localizedBackOfficeMessage;
                }

                return View("~/Views/Cms/login.cshtml", pageModel);
            }

            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("returnUrl")))
            {
                string returnUrl = HttpContext.Session.GetString("returnUrl").ToString();
                HttpContext.Session.Remove("returnUrl");
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("setup", "cms");
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/auth/register")]
        public async Task<IActionResult> RegisterAsync( RegisterPage model )
        {
            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            string refererUrl = Request.Headers["Referer"].ToString();

#if !DEBUG
            if (!string.IsNullOrWhiteSpace(Request.Form["g-recaptcha-response"]))
            {
                var recaptchaResponse = await _recaptcha.Validate(Request.Form["g-recaptcha-response"]);
                if (!recaptchaResponse.success)
                {
                    TempData["StatusMessage"] = AuthHelper.GetBackOfficeStringLocalized(userCulture, "captcha incorrect");
                    return Redirect(refererUrl);
                }
            }
            else
            {
                TempData["StatusMessage"] = AuthHelper.GetBackOfficeStringLocalized(userCulture, "captcha incorrect");
                return Redirect(refererUrl);
            }
#endif

            model.Email = model.Email.ToLower();
            model.Subdomain = model.Subdomain.ToLower();

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", model.Email },
                { "userPlanName", "Free" },
                { "subDomainName", model.Subdomain },
                { "businessUnitName", model.BusinessUnitName }
            };

            // Attempt to create an account if everything is ok
            var createAccountRequest = await _apiHelper.SendApiCallAsync(
                ApiCalls.CreateAccount,
                urlParameters,
                password: model.Password
            );
            string createAccountResponse = await createAccountRequest.Content.ReadAsStringAsync();
            if ( Guid.TryParse(createAccountResponse, out Guid apiLoginKey) )
            {
                var expirationOffset = new DateTimeOffset().ToOffset(TimeSpan.FromMinutes(20));
                HttpContext.Session.SetString("ApiLoginKey", createAccountResponse);
                CookieHelper.SetCookie("ApiLoginKey",
                    createAccountResponse,
                    HttpContext, options: new CookieOptions { Expires = expirationOffset }
                );
            }

            TempData["StatusCode"] = createAccountRequest.StatusCode;
            TempData["StatusMessage"] = "";

            if ( !createAccountRequest.IsSuccessStatusCode )
            {
                string localizedBackOfficeMessage = AuthHelper.GetBackOfficeStringLocalized(userCulture, createAccountResponse);
                if ( localizedBackOfficeMessage != null )
                {
                    TempData["StatusMessage"] = localizedBackOfficeMessage;
                }
                return Redirect(refererUrl);
            }

            return RedirectToAction("setup", "cms");
        }

        [HttpPost("/auth/getUserKey")]
        public async Task<JsonResult> GetUserKey()
        {
            var apiLoginKey = HttpContext.Session.GetString("ApiLoginKey") ?? CookieHelper.GetCookie("ApiLoginKey", HttpContext);
            if (apiLoginKey == null)
            {
                return new JsonResult( new { statusCode = HttpStatusCode.BadRequest });
            }
            var email = await _apiHelper.GetEmailFromLoginKeyAsync(apiLoginKey, HttpContext);
            var getUserKeyParameters = new Dictionary<string, string>() {
                    { "email", email }
                };

            var subdomainResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.UserSubDomain,
                getUserKeyParameters,
                context: HttpContext
            );

            var subdomain = await subdomainResponse.Content.ReadAsStringAsync();
            var redirectUrl = $"{BackOffice.Protocol}://{subdomain}.{BackOffice.HostName.Replace("/api","")}";

            // Add subDomainName to the list of parameters because we need it in the next call
            getUserKeyParameters.Add("subDomainName", subdomain);

            var userKeyResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.GetRentVisionLoginKey,
                getUserKeyParameters,
                context: HttpContext
            );

            string userKey = await userKeyResponse.Content.ReadAsStringAsync();
            string realRedirectUrl = redirectUrl + "/externLogin?externLoginKey=" + userKey;

            return new JsonResult( new { statusCode = HttpStatusCode.OK, realRedirectUrl } );
        }
    }
}