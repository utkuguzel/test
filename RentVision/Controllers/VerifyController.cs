using Microsoft.AspNetCore.Mvc;
using Piranha;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Twinvision.Piranha.RentVision.Helpers;
using RentVision.Models.Configuration;
using System.Collections.Generic;
using RentVision.Models;
using Microsoft.AspNetCore.Localization;
using Twinvision.Piranha.RentVision.Resources;
using System.Globalization;
using RentVision.Helpers;
using Microsoft.AspNetCore.Http;
using static RentVision.Models.Configuration.Configuration;

namespace RentVision.Controllers
{
    /// <summary>
    /// This controller handles the verification of each step in Setup
    /// </summary>
    [Route("[controller]")]
    public class VerifyController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;
        private ApiHelper _apiHelper;

        public VerifyController(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper(_api, _clientFactory);
        }

        [HttpPost("form/register")]
        public JsonResult VerifyRegisterForm(RegisterForm form)
        {
            // FluentValidation bug fix, should refactor
            var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
            Strings.Culture = new CultureInfo(rqf.RequestCulture.Culture.TwoLetterISOLanguageName);

            var validator = new RegisterPageValidator();
            var validationResult = validator.Validate(form);
            if (validationResult.IsValid)
            {
                return Json(HttpStatusCode.OK);
            }
            return Json(validationResult.Errors);
        }

        [HttpPost("form/login")]
        public JsonResult VerifyLoginForm(LoginForm form)
        {
            // FluentValidation bug fix, should refactor
            var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
            Strings.Culture = new CultureInfo(rqf.RequestCulture.Culture.TwoLetterISOLanguageName);

            var validator = new LoginPageValidator();
            var validationResult = validator.Validate(form);
            if (validationResult.IsValid)
            {
                return Json(HttpStatusCode.OK);
            }
            return Json(validationResult.Errors);
        }

        [HttpPost("createVerificationCodeEmail/{email}")]
        public async Task<JsonResult> CreateVerificationCodeEmail(string email)
        {
            string userCulture = CultureHelper.GetUserCulture(Request, HttpContext);
            var verificationCodeParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "culture", userCulture }
            };
            var verificationCodeResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.CreateVerificationCode,
                verificationCodeParameters,
                context: HttpContext
            );
            var verificationCodeMessage = await verificationCodeResponse.Content.ReadAsStringAsync();
            if (!verificationCodeResponse.IsSuccessStatusCode)
            {
                return new JsonResult(new { statusCode = verificationCodeResponse.StatusCode, statusMessage = verificationCodeMessage });
            }
            return new JsonResult(new { statusCode = HttpStatusCode.OK, statusMessage = verificationCodeMessage });
        }

        [HttpPost("code/{email}")]
        public async Task<JsonResult> Verify(string email)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
            };
            var verificationCodeResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.GetVerificationCodeStatus,
                urlParameters,
                context: HttpContext
            );
            var responseString = await verificationCodeResponse.Content.ReadAsStringAsync();
            return new JsonResult(new { verificationCodeResponse.StatusCode, responseString });
        }

        [HttpPost("code/{email}/{code}")]
        public async Task<JsonResult> VerifyCodeAsync(string email, string code)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email },
                { "code", code }
            };
            var verificationCodeResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.SetVerificationCodeVerified,
                urlParameters,
                context: HttpContext
            );
            var responseString = await verificationCodeResponse.Content.ReadAsStringAsync();
            return new JsonResult(new { verificationCodeResponse.StatusCode, responseString });
        }

        [HttpPost("transaction/{transactionId}")]
        public async Task<JsonResult> VerifyTransactionAsync(string transactionId)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "id", transactionId }
            };
            var transactionResponse = await _apiHelper.SendApiCallAsync(
                ApiCalls.GetTransactionStatus,
                urlParameters,
                context: HttpContext);
            var paymentStatus = await transactionResponse.Content.ReadAsStringAsync();

            if (paymentStatus == "Paid")
                HttpContext.Session.Remove("paymentId");

            return new JsonResult(new { transactionResponse.StatusCode, paymentStatus });
        }

        [HttpPost("environment")]
        public JsonResult VerifyEnvironment(string email)
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}