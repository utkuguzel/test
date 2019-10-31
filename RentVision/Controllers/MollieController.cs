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
using RentVision;
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

        public static string GetMollieKey()
        {
            var mollieSettings = Startup.Config.GetSection("MollieSettings");
            if (bool.TryParse(mollieSettings["useTestKey"], out bool testKeyEnabled))
            {
                if (testKeyEnabled)
                {
                    return mollieSettings["apiKeyTest"];
                }
            }
            return mollieSettings["apiKeyLive"];
        }
    }
}