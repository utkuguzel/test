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
            var userDetails = new Dictionary<string, string>()
            {
                { "email", email },
                { "password", password },
            };

            var response = await SendApiCallAsync(Configuration.ApiCalls.CheckUserCredentials, userDetails, HttpMethod.Post);

            if (response.IsSuccessStatusCode)
            {
                //success
            }
            else
            {
                //error
            }

            return new JsonResult(new { httpStatusCode = response.StatusCode, httpStatusMessage = await response.Content.ReadAsStringAsync() });
        }

        public async Task<HttpResponseMessage> SendApiCallAsync(string callType, Dictionary<string, string> data, HttpMethod callMethod)
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

            var client = _clientFactory.CreateClient("RentVisionApi");

            var response = await client.SendAsync(request);

            return response;
        }

        [Route("/auth/register"), HttpPost]
        public IActionResult Register()
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}
