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

        /// <summary>
        /// Gets the blog archive with the given id.
        /// </summary>
        /// <param name="id">The unique page id</param>
        /// <param name="year">The optional year</param>
        /// <param name="month">The optional month</param>
        /// <param name="page">The optional page</param>
        /// <param name="category">The optional category</param>
        /// <param name="tag">The optional tag</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("archive")]
        public async Task<IActionResult> Archive(Guid id, int? year = null, int? month = null, int? page = null,
            Guid? category = null, Guid? tag = null, bool draft = false)
        {
            var model = await _loader.GetPage<BlogArchive>(id, HttpContext.User, draft);
            model.Archive = await _api.Archives.GetByIdAsync(id, page, category, tag, year, month);

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the page with the given id.
        /// </summary>
        /// <param name="id">The unique page id</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("page")]
        public async Task<IActionResult> Page(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<StandardPage>(id, HttpContext.User, draft);

            // TEST
            var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture.TwoLetterISOLanguageName;

            var sites = await _api.Sites.GetAllAsync();
            var site = sites.SingleOrDefault(m => m.Culture == culture);

            var pageResult = await _api.Pages.GetAllAsync<StandardPage>(siteId: site.Id);

            model = await _api.Pages.GetByIdAsync<StandardPage>(pageResult.FirstOrDefault(m => m.Slug == model.Slug).Id);
            // END TEST

            // Return user to proper culture page
            //string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            //if (cultureUrl != null)
            //{
            //    return LocalRedirect(cultureUrl);
            //}

            return View(model);
        }

        /// <summary>
        /// Gets the post with the given id.
        /// </summary>
        /// <param name="id">The unique post id</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("post")]
        public async Task<IActionResult> Post(Guid id, bool draft = false)
        {
            var model = await _loader.GetPost<BlogPost>(id, HttpContext.User, draft);

            // Return user to proper culture page
            //string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            //if (cultureUrl != null)
            //{
            //    return LocalRedirect(cultureUrl);
            //}

            return View(model);
        }

    
        /// <summary>
        /// Gets the startpage with the given id.
        /// </summary>
        /// <param name="id">The unique page id</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("start")]
        public async Task<IActionResult> Start(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<StartPage>(id, HttpContext.User, draft);

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the loginpage with the given id.
        /// </summary>
        /// <param name="id">The unique page id</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("login")]
        public async Task<IActionResult> Login(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<LoginPage>(id, HttpContext.User, draft);

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the registerpage with the given id.
        /// </summary>
        /// <param name="id">The unique page id</param>
        /// <param name="draft">If a draft is requested</param>
        [Route("register")]
        public async Task<IActionResult> Register(Guid id, bool draft = false, string userPlan = "Free", string payInterval = "2")
        {
            var model = await _loader.GetPage<RegisterPage>(id, HttpContext.User, draft);

            if ( HttpContext.Session.GetString("UserPlan") == null )
            {
                HttpContext.Session.SetString("UserPlan", userPlan);
                HttpContext.Session.SetString("PayInterval", payInterval);
            }

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        [Route("product")]
        public async Task<IActionResult> Product(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<ProductPage>(id, HttpContext.User, draft);

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        [Route("plans")]
        public async Task<IActionResult> Plans(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<PlansPage>(id, HttpContext.User, draft);

            HttpContext.Session.Remove("UserPlan");
            HttpContext.Session.Remove("PayInterval");

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);

            if (cultureUrl != null)
            {
                return LocalRedirect(cultureUrl);
            }

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
                var customerClient = new CustomerClient(CustomerController.MollieKeyTest);
                var customerList = await customerClient.GetCustomerListAsync();
                var customer = customerList.Items.FirstOrDefault(c => c.Email == email);
                if (customer != null)
                {
                    var paymentClient = new PaymentClient(CustomerController.MollieKeyTest);
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
    }
}
