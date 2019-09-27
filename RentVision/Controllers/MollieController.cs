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

        [HttpPost("getTransactionId")]
        public async Task<JsonResult> GetMollieTransactionIdAsync()
        {
            var mollieId = Request.Form["id"];
            var urlParameters = new Dictionary<string, string>()
            {
                { "id", mollieId }
            };

            var paymentWebhookResponse = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.PaymentWebhook, HttpMethod.Post, urlParameters, context: HttpContext);
            var responseResult = paymentWebhookResponse.Content.ReadAsStringAsync();

            if ( !paymentWebhookResponse.IsSuccessStatusCode )
            {
                return new JsonResult(new { HttpStatusCode.BadRequest, responseResult });
            }

            return new JsonResult(new { HttpStatusCode.OK, responseResult });
        }

        [HttpGet("test")]
        public JsonResult Test()
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}