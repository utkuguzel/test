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

            if ( !response.IsSuccessStatusCode )
            {
                ViewBag.StatusCode = response.StatusCode;
                ViewBag.StatusMessage = await response.Content.ReadAsStringAsync();

                // TODO: ViewBag fixen

                return RedirectToAction( "login", "cms" );
            }

            // Subdomein ophalen en doorsturen

            return Redirect("https://google.com");
        }

        [Route("/auth/register"), HttpPost]
        public IActionResult Register()
        {
            return new JsonResult(HttpStatusCode.OK);
        }

        /// <summary>
        /// Sends an async api call to the rentvision backoffice api and returns an HttpResponseMessage object
        /// </summary>
        /// <param name="callType">The type of call that should be called (Mapped to Configuration.BackOffice or ApiCalls)</param>
        /// <param name="data">A Dictionary containing parameter data</param>
        /// <param name="callMethod">The HttpMethod that should be used to send data</param>
        /// <returns>An HttpResponseMessage containing response data from the API</returns>
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
    }
}
