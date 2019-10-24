using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Piranha;
using RentVision.Models.Configuration;
using Twinvision.Piranha.RentVision.Helpers;

namespace Twinvision.Piranha.RentVision.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MollieController : ControllerBase
    {
        public ApiHelper _apiHelper { get; set; }

        public MollieController(IApi api, IHttpClientFactory clientFactory)
        {
            _apiHelper = new ApiHelper(api, clientFactory);
        }

        // This method will be triggered by the Mollie Webhook
        // And sends the transactionId to the centralized API for further processing.
        [HttpPost("updateTransaction")]
        public async Task<ActionResult> UpdateTransactionAsync()
        {
            var mollieId = Request.Form["id"];
            var urlParameters = new Dictionary<string, string>()
            {
                { "id", mollieId }
            };

            var paymentWebhookResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.PaymentWebhook, HttpMethod.Post, urlParameters);
            var responseResult = await paymentWebhookResponse.Content.ReadAsStringAsync();

            if ( !paymentWebhookResponse.IsSuccessStatusCode )
            {
                return BadRequest(responseResult);
            }

            return Ok();
        }

        [HttpPost("updateSubscription")]
        public async Task<ActionResult> UpdateSubscriptionAsync()
        {
            var mollieId = Request.Form["id"];
            var urlParameters = new Dictionary<string, string>()
            {
                { "id", mollieId }
            };

            var paymentWebhookResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.SubscriptionWebhook, HttpMethod.Post, urlParameters);
            var responseResult = await paymentWebhookResponse.Content.ReadAsStringAsync();

            if (!paymentWebhookResponse.IsSuccessStatusCode)
            {
                return BadRequest(responseResult);
            }

            return Ok();
        }
    }
}