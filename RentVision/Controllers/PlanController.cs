using Microsoft.AspNetCore.Mvc;
using Piranha;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Twinvision.Piranha.RentVision.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using static RentVision.Models.Configuration.Configuration;
using Newtonsoft.Json;
using System.Linq;

namespace RentVision.Controllers
{
    public enum PaymentInterval : int
    {
        Yearly = 1,
        Montly = 2
    }

    public class PlanFeature
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
        public string Culture { get; set; }
    }

    public class Plan
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public PaymentInterval PayInterval { get; set; }
    }

    [Route("/api/plans")]
    public class PlanController : Controller
    {
        private readonly IApi _api;
        private readonly IHttpClientFactory _clientFactory;
        private ApiHelper _apiHelper;

        public PlanController(IApi api, IHttpClientFactory clientFactory)
        {
            _api = api;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper(_api, _clientFactory);
        }

        [HttpGet()]
        public async Task<List<Plan>> GetPlansListAsync()
        {
            var response = await _apiHelper.SendApiCallAsync(ApiCalls.GetPlans);
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Plan>>(responseData);
        }

        [HttpGet("{pid:guid}")]
        public async Task<Plan> GetPlanAsync(Guid pid)
        {
            var plans = await GetPlansListAsync();
            return plans.FirstOrDefault(p => p.Id == pid);
        }

        [HttpGet("features")]
        public async Task<List<PlanFeature>> GetPlanFeaturesAsync()
        {
            var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture.TwoLetterISOLanguageName;
            var urlParameters = new Dictionary<string, string>()
            {
                { "culture", culture }
            };
            var response = await _apiHelper.SendApiCallAsync(ApiCalls.GetPlanFeatures, urlParameters);
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PlanFeature>>(responseData);
        }
    }
}