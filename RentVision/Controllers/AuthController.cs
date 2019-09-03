using RentVision.Models;
using RentVision.Models.Regions;
using Microsoft.AspNetCore.Mvc;
using Piranha;
using Piranha.Extend.Blocks;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace RentVision.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
        }

        [Route("/auth/login"), HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password, bool remember)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"RentVisionAccount/CheckUserCredentials?email={email}&password={password}"
            );

            var client = _clientFactory.CreateClient("RentVisionApi");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                //success
            }
            else
            {
                //error
            }

            return new JsonResult( new { httpStatusCode = response.StatusCode, httpStatusMessage = await response.Content.ReadAsStringAsync() } );
        }

        [Route("/auth/register"), HttpPost]
        public IActionResult Register()
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}
