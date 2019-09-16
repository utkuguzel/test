using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Piranha;
using RentVision.Models;
using RentVision.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Twinvision.Piranha.RentVision.Helpers
{
    public class ApiHelper
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;

        public ApiHelper(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Sends an async api call to the rentvision backoffice api and returns an HttpResponseMessage object
        /// </summary>
        /// <param name="callType">The type of call that should be called (Mapped to Configuration.BackOffice or ApiCalls)</param>
        /// <param name="data">A Dictionary containing parameter data</param>
        /// <param name="callMethod">The HttpMethod that should be used to send data</param>
        /// <returns>An HttpResponseMessage containing response data from the API</returns>
        public async Task<HttpResponseMessage> SendApiCallAsync(string callType, HttpMethod callMethod, Dictionary<string, string> data = null, string password = null)
        {
            data = data ?? new Dictionary<string, string>();

            var query = QueryHelpers.AddQueryString(
                $"{Configuration.BackOffice.Protocol}://{Configuration.BackOffice.HostName}/{callType}",
                data
            );

            var request = new HttpRequestMessage
            {
                Method = callMethod,
                RequestUri = new Uri(query)
            };

            if (password != null)
            {
                request.Headers.Add("X-Password", password);
            }

            var client = _clientFactory.CreateClient("RentVisionApi");

            var response = await client.SendAsync(request);

            return response;
        }

        public async Task<List<UserPlan>> GetUserPlansAsync()
        {
            var response = await SendApiCallAsync(Configuration.ApiCalls.UserPlans, HttpMethod.Get);
            var responseData = await response.Content.ReadAsStringAsync();

            List<UserPlan> userPlans = JsonConvert.DeserializeObject<List<UserPlan>>(responseData);

            return userPlans;
        }
    }
}
