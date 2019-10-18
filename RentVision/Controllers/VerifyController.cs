using Microsoft.AspNetCore.Mvc;
using Piranha;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Twinvision.Piranha.RentVision.Helpers;
using RentVision.Models.Configuration;
using System.Collections.Generic;

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

        [HttpPost("code/{email}")]
        public async Task<JsonResult> Verify(string email)
        {
            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
            };
            var verificationCodeResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.GetVerificationCodeStatus,
                HttpMethod.Get, urlParameters,
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
                Configuration.ApiCalls.SetVerificationCodeVerified,
                HttpMethod.Post, urlParameters,
                context: HttpContext
            );
            var responseString = await verificationCodeResponse.Content.ReadAsStringAsync();
            return new JsonResult(new { verificationCodeResponse.StatusCode, responseString });
        }

        [HttpPost("transaction")]
        public async Task<JsonResult> VerifyTransactionAsync(string transactionId)
        {
            // TODO: TransactionRecords table aanmaken

            return new JsonResult(HttpStatusCode.OK);
        }

        [HttpPost("environment")]
        public async Task<JsonResult> VerifyEnvironmentAsync(string email)
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}