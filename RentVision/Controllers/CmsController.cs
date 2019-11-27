using RentVision.Models;
using Microsoft.AspNetCore.Mvc;
using Piranha;
using Piranha.AspNetCore.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RentVision.Helpers;
using RentVision.Models.Regions;
using System.Net;
using Newtonsoft.Json;
using Twinvision.Piranha.RentVision.Helpers;
using System.Net.Http;
using RentVision.Models.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using System.Linq;
using Twinvision.Piranha.RentVision.Controllers;
using Mollie.Api.Client;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Url;
using Mollie.Api.Models.Payment.Response;
using System.Security.Claims;
using Piranha.Models;
using System.Threading;
using Twinvision.Piranha.RentVision.Resources;
using static RentVision.Models.Configuration.Configuration;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models;
using System.Reflection;
using Mollie.Api.Models.Customer;

namespace RentVision.Controllers
{
    public class CmsController : Controller
    {
        private readonly IApi _api;
        private readonly IModelLoader _loader;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiHelper _apiHelper;

        public CmsController(IApi api, IModelLoader loader, IHttpClientFactory clientFactory)
        {
            _api = api;
            _loader = loader;
            _clientFactory = clientFactory;
            _apiHelper = new ApiHelper(_api, _clientFactory);
        }

        [Route("archive")]
        public async Task<IActionResult> Archive(Guid id, int? year = null, int? month = null, int? page = null,
            Guid? category = null, Guid? tag = null, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<BlogArchive>(id, HttpContext.User, draft);
            model.Archive = await _api.Archives.GetByIdAsync(id, page, category, tag, year, month);
            return View(model);
        }

        [Route("page")]
        public async Task<IActionResult> Page(Guid id, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<StandardPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("post")]
        public HttpStatusCode Post(Guid id, bool draft = false)
        {
            return HttpStatusCode.OK;
        }

        [Route("start")]
        public async Task<IActionResult> Start(Guid id, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<StartPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("login")]
        public async Task<IActionResult> Login(Guid id, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<LoginPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("register/{pid:guid?}")]
        public async Task<IActionResult> Register(Guid id, Guid pid, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<RegisterPage>(id, HttpContext.User, draft);
            var planController = new PlanController(_api, _clientFactory);
            var plans = await planController.GetPlansListAsync();
            var selectedPlan = plans.FirstOrDefault(plan => plan.Name.ToLower().IndexOf("free") != -1);
            if (pid != Guid.Empty)
            {
                selectedPlan = plans.FirstOrDefault(plan => plan.Id == pid);
            }
            if (HttpContext.Session.GetString("UserPlan") == null)
            {
                HttpContext.Session.SetString("UserPlan", selectedPlan.Id.ToString());
            }

            model.SelectedUserPlan = selectedPlan;
            return View(model);
        }

        [Route("product")]
        public async Task<IActionResult> Product(Guid id, bool draft = false)
        {
            var model = await GetCulturizedModelAsync<ProductPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("plans")]
        public async Task<IActionResult> Plans(Guid id, bool draft = false)
        {
            HttpContext.Session.Remove("UserPlan");

            var model = await GetCulturizedModelAsync<PlansPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("setup/{code?}")]
        public async Task<IActionResult> Setup(Guid id, string code, bool draft = false)
        {
            CustomerResponse customer = null;
            var customerController = new CustomerController(_api, _clientFactory);
            string email = null;
            string apiLoginKey = HttpContext.Session.GetString("ApiLoginKey") ?? CookieHelper.GetCookie("ApiLoginKey", HttpContext);
            if (apiLoginKey != null)
            {
                email = await _apiHelper.GetEmailFromLoginKeyAsync(apiLoginKey, HttpContext);
            }

            if (email == null)
            {
                var returnUrl = !string.IsNullOrWhiteSpace(code) ? $"/setup/{code}" : "/setup";
                HttpContext.Session.SetString("returnUrl", returnUrl);
                return LocalRedirect("/login");
            }

            var sessionUserPlan = HttpContext.Session.GetString("UserPlan") ?? null;
            if (!Guid.TryParse(sessionUserPlan, out Guid userPlanId))
            {
                customer = await customerController.GetCustomerAsync(email);
                if (customer != null)
                {
                    var latestPayment = await customerController.GetLatestCustomerPayment(customer.Id);
                    if (latestPayment != null)
                    {
                        userPlanId = latestPayment.GetMetadata<UserPlanMetaData>().Plan.Id;
                    }
                    else
                    {
                        return LocalRedirect("/plans");
                    }
                }
            }

            var planListRequest = await _apiHelper.SendApiCallAsync(ApiCalls.GetPlans);
            var planListResponse = await planListRequest.Content.ReadAsStringAsync();
            var planList = JsonConvert.DeserializeObject<List<Controllers.Plan>>(planListResponse);
            var userPlan = planList.FirstOrDefault(p => p.Id == userPlanId);

            if (userPlan != null)
            {
                if (customer == null)
                {
                    customer = await customerController.CreateCustomerAsync(email, email, HttpContext);
                }

                var model = await _loader.GetPage<SetupPage>(id, HttpContext.User, draft);
                model.Email = email;
                model.SelectedPlan = userPlan;
                if (model.SelectedPlan.Price > 0)
                {
                    var latestPayment = await customerController.GetLatestCustomerPayment(customer.Id);
                    PaymentResponse paymentResponse = latestPayment;
                    if (latestPayment == null)
                    {
                        paymentResponse = await customerController.CreatePaymentRequestAsync(userPlan, email, customer.Id, HttpContext);
                    }
                    var latestPaymentMetadata = paymentResponse.GetMetadata<UserPlanMetaData>();
                    if (paymentResponse.Status == PaymentStatus.Open )
                    {
                        model.MollieCheckoutUrl = paymentResponse.Links.Checkout.Href;
                    }
                    else if (paymentResponse.Status != PaymentStatus.Paid)
                    {
                        model.MollieCheckoutUrl = $"/customer/payment/create/{userPlan.Id}/{customer.Id}";
                    }
                    model.MolliePaymentId = paymentResponse.Id;
                    model.paymentStatus = paymentResponse.Status;
                    model.IsUpgrade = latestPaymentMetadata.IsUpgrade;
                    model.UpgradePrice = latestPaymentMetadata.UpgradePrice;
                }

                if (!string.IsNullOrWhiteSpace(code))
                {
                    model.Code = code;
                }

                HttpContext.Session.Remove("UserPlan");

                return View(model);
            }

            return LocalRedirect("/");
        }

        private async Task<(string checkoutUrl, string paymentId, bool IsUpgrade, string UpgradePrice)> GenerateMollieCheckoutUrl(string email, Plan userPlan, string businessUnitName, HttpContext context)
        {
            var customerController = new CustomerController(_api, _clientFactory);
            var mollieResponse = await _apiHelper.SendApiCallAsync(ApiCalls.GetMollieId, context: HttpContext);
            var mollieId = await mollieResponse.Content.ReadAsStringAsync();
            if (!mollieResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve customer MollieId");
            }
            var paymentListResponse = await customerController.GetPaymentListAsync();
            var customerPayments = paymentListResponse.Where(m => m.CustomerId == mollieId).ToList();
            if (customerPayments.Count <= 0)
            {
                var checkoutUrl = await customerController.CreatePaymentRequestAsync(userPlan, email, mollieId, HttpContext);
                if (checkoutUrl != null)
                {
                    return (checkoutUrl.Links.Checkout.Href, checkoutUrl.Id, false, null);
                }
            }
            else
            {
                var openPayment = customerPayments.FirstOrDefault(m => m.Status == PaymentStatus.Open);
                if (openPayment != null)
                {
                    var openPaymentMetaData = openPayment.GetMetadata<UserPlanMetaData>();
                    return (openPayment.Links.Checkout.Href, openPayment.Id, openPaymentMetaData.IsUpgrade, openPaymentMetaData.UpgradePrice);
                }
            }

            return (null, null, false, null);
        }

        [Route("/api/killAllSites")]
        public async Task<IActionResult> KillAllSites()
        {
            var result = await _apiHelper.SendApiCallAsync(ApiCalls.KillAllSites);
            var resultString = await result.Content.ReadAsStringAsync();
            return new JsonResult(new { result.StatusCode, resultString });
        }

        //[Route("/api/benchmarkCreateAccount/{num}")]
        //public async Task<ActionResult> BenchmarkCreateAccount(string num)
        //{
        //    var urlParameters = new Dictionary<string, string>()
        //        {
        //            { "email", $"test{num}@test.com" },
        //            { "userPlanName", "Free" },
        //            { "subDomainName", $"testdomain{num}" },
        //            { "businessUnitName", $"Test B.V." }
        //        };

        //    // Attempt to create an account if everything is ok
        //    var response = await _apiHelper.SendApiCallAsync(
        //        Configuration.ApiCalls.CreateAccount,
        //        HttpMethod.Post,
        //        urlParameters,
        //        "Welkom123!"
        //    );
        //    var responseMessage = await response.Content.ReadAsStringAsync();

        //    return new JsonResult( new { response.StatusCode, responseMessage });
        //}

        //public async Task<T> GetCulturizedPostModelAsync<T>(Guid id, ClaimsPrincipal user, bool draft)
        //    where T : PostBase
        //{
        //    var model = await _loader.GetPost<T>(id, HttpContext.User, draft);

        //    var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
        //    var culture = rqf.RequestCulture.Culture.TwoLetterISOLanguageName;
        //    var sites = await _api.Sites.GetAllAsync();
        //    var site = sites.SingleOrDefault(m => m.Culture == culture);
        //    var pageResult = await _api.Posts.GetAllAsync<T>(blogId: id);
        //    return await _api.Posts.GetByIdAsync<T>(pageResult.FirstOrDefault(m => m.Slug == model.Slug).Id);
        //}

        public async Task<T> GetCulturizedModelAsync<T>(Guid id, ClaimsPrincipal user, bool draft)
            where T : PageBase
        {
            var model = await _loader.GetPage<T>(id, HttpContext.User, draft);

            var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture.TwoLetterISOLanguageName;
            var sites = await _api.Sites.GetAllAsync();
            var site = sites.SingleOrDefault(m => m.Culture == culture);
            var pageResult = await _api.Pages.GetAllAsync<T>(siteId: site.Id);

            return await _api.Pages.GetByIdAsync<T>(pageResult.FirstOrDefault(m => m.Slug == model.Slug).Id);
        }
    }
}
