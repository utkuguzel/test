using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Mollie.Api.Models.Customer;
using Newtonsoft.Json;
using Piranha;
using RentVision.Helpers;
using RentVision.Models;
using RentVision.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static RentVision.Models.Configuration.Configuration;

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
        public async Task<HttpResponseMessage> SendApiCallAsync(ApiCall call, Dictionary<string, string> data = null, string password = null, HttpContext context = null)
        {
            data = data ?? new Dictionary<string, string>();

            var query = QueryHelpers.AddQueryString(
                $"{BackOffice.Url}/{Enum.GetName(typeof(ApiGroup), call.ApiCategory)}/{call.Url}",
                data
            );

            var request = new HttpRequestMessage
            {
                Method = call.Method,
                RequestUri = new Uri(query)
            };

            if (password != null)
            {
                request.Headers.Add("X-Password", password);
            }

            if (context != null && call.Url != ApiCalls.LoginUserRentVisionApi.Url)
            {
                var apiLoginKey = context.Session.GetString("ApiLoginKey") ?? CookieHelper.GetCookie("ApiLoginKey", context);
                if (apiLoginKey != null)
                {
                    request.Headers.Add("X-ApiLoginKey", apiLoginKey);
                }
                else
                {
                    throw new Exception("ApiLoginKey cookie is null");
                }
            }

            var client = _clientFactory.CreateClient("RentVisionApi");
            var response = await client.SendAsync(request);
            return response;
        }

        public async Task<List<UserPlan>> GetUserPlansAsync()
        {
            var response = await SendApiCallAsync(ApiCalls.UserPlans);
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserPlan>>(responseData);
        }

        public async Task<string> GetEmailFromLoginKeyAsync(string apiLoginKey, HttpContext context)
        {
            var GetEmailParameters = new Dictionary<string, string>()
                {
                    { "ApiLoginKey", apiLoginKey }
                };
            var emailResponse = await SendApiCallAsync(
                ApiCalls.GetEmail,
                GetEmailParameters,
                context: context
            );
            if (emailResponse.IsSuccessStatusCode)
            {
                return await emailResponse.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}
