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
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;
        private ApiHelper _apiHelper;

        public AccountController(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper(_api, _clientFactory);
        }

        [HttpPost("/verify/email/{email}")]
        public async Task<JsonResult> VerifyEmailAsync(string email)
        {
            return new JsonResult(HttpStatusCode.OK);
        }

        [HttpPost("/verify/transaction/{transactionId}")]
        public async Task<JsonResult> VerifyTransactionAsync(string transactionId)
        {
            return new JsonResult(HttpStatusCode.OK);
        }

        [HttpPost("/verify/environment/{email}")]
        public async Task<JsonResult> VerifyEnvironmentAsync(string email)
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}