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

namespace RentVision.Controllers
{
    public class CmsController : Controller
    {
        private readonly IApi _api;
        private readonly IModelLoader _loader;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiHelper _apiHelper;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="api">The current api</param>
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
            //var model = await GetCulturizedPostModelAsync<BlogPost>(id, HttpContext.User, draft);
            //return View(model);
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

        [Route("register")]
        public async Task<IActionResult> Register(Guid id, bool draft = false, string userPlan = "Free", string payInterval = "2")
        {
            if ( HttpContext.Session.GetString("UserPlan") == null )
            {
                HttpContext.Session.SetString("UserPlan", userPlan);
                HttpContext.Session.SetString("PayInterval", payInterval);
            }

            var model = await GetCulturizedModelAsync<RegisterPage>(id, HttpContext.User, draft);
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
            HttpContext.Session.Remove("PayInterval");

            var model = await GetCulturizedModelAsync<PlansPage>(id, HttpContext.User, draft);
            return View(model);
        }

        [Route("setup/{code?}")]
        public async Task<IActionResult> Setup(Guid id, string code, bool draft = false)
        {
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
            var sessionUserPlanInterval = HttpContext.Session.GetString("PayInterval") ?? null;
            if ( sessionUserPlan == null || sessionUserPlanInterval == null )
            {
                // Check if user has any payments (OPEN payments will be checked in the GenerateMollieCheckoutUrl method)
                var customerClient = new CustomerClient(MollieController.GetMollieKey());
                var customerList = await customerClient.GetCustomerListAsync();
                var customer = customerList.Items.FirstOrDefault(c => c.Email == email);
                if (customer != null)
                {
                    var paymentClient = new PaymentClient(MollieController.GetMollieKey());
                    var customerPayments = await customerClient.GetCustomerPaymentListAsync(customer.Id);
                    if (customerPayments.Items.Count > 0)
                    {
                        var userPlanMetaData = customerPayments.Items.FirstOrDefault().GetMetadata<UserPlanMetaData>();
                        if (userPlanMetaData != null)
                        {
                            sessionUserPlan = userPlanMetaData.Plan.Name;
                            sessionUserPlanInterval = userPlanMetaData.Plan.PayInterval.ToString();
                        }
                    }
                    else
                    {
                        // else redirect to plans page again
                        return LocalRedirect("/plans");
                    }
                }
            }

            var userPlanList = await _apiHelper.GetUserPlansAsync();
            var userPlan = userPlanList.FirstOrDefault(p => p.Name.IndexOf(sessionUserPlan) != -1 && p.PayInterval == Convert.ToInt32(sessionUserPlanInterval));

            if ( userPlan != null )
            {
                var model = await _loader.GetPage<SetupPage>(id, HttpContext.User, draft);
                model.Email = email;
                model.SelectedPlan = userPlan;
                if(model.SelectedPlan.Price > 0)
                {
                    var checkoutData = await GenerateMollieCheckoutUrl(email, model.SelectedPlan, email, HttpContext);
                    model.MollieCheckoutUrl = checkoutData.checkoutUrl;
                    model.MolliePaymentId = checkoutData.paymentId;
                }
                if (!string.IsNullOrWhiteSpace(code))
                {
                    model.Code = code;
                }

                return View(model);
            }

            return LocalRedirect("/");
        }
        
        private async Task<(string checkoutUrl, string paymentId)> GenerateMollieCheckoutUrl(string email, UserPlan userPlan, string businessUnitName, HttpContext context)
        {
            var customerController = new CustomerController(_api, _clientFactory);
            var customerList = await customerController.GetCustomerListAsync();
            PaymentResponse checkoutUrl;
            if ( !customerController.DoesCustomerExist(email, customerList) )
            {
                var response = await customerController.CreateCustomerAsync(email, businessUnitName, context);
                var mollieCustomerId = response.Value.ToString();
                checkoutUrl = await customerController.CreatePaymentRequest(userPlan, email, mollieCustomerId, HttpContext);
            }
            else
            {
                var mollieResponse = await _apiHelper.SendApiCallAsync(
                    Configuration.ApiCalls.GetMollieId,
                    HttpMethod.Get,
                    context: HttpContext);
                var mollieId = await mollieResponse.Content.ReadAsStringAsync();
                if ( !mollieResponse.IsSuccessStatusCode )
                {
                    throw new Exception("Failed to retrieve customer MollieId");
                }
                var paymentListResponse = await customerController.GetPaymentListAsync();
                var customerPayments = paymentListResponse.Items.Where(m => m.CustomerId == mollieId).ToList();
                // Customer exists but there are no payments
                if ( customerPayments.Count <= 0 )
                {
                    checkoutUrl = await customerController.CreatePaymentRequest(userPlan, email, mollieId, HttpContext);
                }
                else
                {
                    // Check if there are any open payments, and if so return one
                    var openPayment = customerPayments.FirstOrDefault(m => m.Status == PaymentStatus.Open);
                    if ( openPayment != null )
                    {
                        return (openPayment.Links.Checkout.Href, openPayment.Id);
                    }
                }

                return (null, null);
            }

            return (checkoutUrl.Links.Checkout.Href, checkoutUrl.Id);
        }

        [Route("/api/getUserPlans")]
        public async Task<JsonResult> GetUserPlansAsync()
        {
            List<UserPlan> userPlans = await _apiHelper.GetUserPlansAsync();
            return new JsonResult(userPlans);
        }

        [HttpGet("paid")]
        public ActionResult Paid()
        {
            HttpContext.Session.Remove("UserPlan");
            HttpContext.Session.Remove("PayInterval");
            return View();
        }

        [Route("/api/killAllSites")]
        public async Task<IActionResult> KillAllSites()
        {
            var result = await _apiHelper.SendApiCallAsync(Configuration.ApiCalls.KillAllSites, HttpMethod.Post);
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
