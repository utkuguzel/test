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

namespace RentVision.Controllers
{
    public class CmsController : Controller
    {
        private readonly IApi _api;
        private readonly IModelLoader _loader;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiHelper _apiHelper;
        private bool _DEBUG = false;

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

            if ( userPlan != "Free" )
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
                var GetEmailFromLoginKeyParameters = new Dictionary<string, string>()
                {
                    { "ApiLoginKey", apiLoginKey }
                };
                var emailResponse = await _apiHelper.SendApiCallAsync(
                    Configuration.ApiCalls.GetEmailFromLoginKey,
                    HttpMethod.Get,
                    GetEmailFromLoginKeyParameters,
                    context: HttpContext
                );
                if (emailResponse.IsSuccessStatusCode)
                {
                    email = await emailResponse.Content.ReadAsStringAsync();
                }
            }

            if (email == null)
                return LocalRedirect("/login");

            var urlParameters = new Dictionary<string, string>()
            {
                { "email", email }
            };
            var singleUserPlanResponse = await _apiHelper.SendApiCallAsync(
                Configuration.ApiCalls.SingleUserPlan,
                HttpMethod.Get,
                urlParameters,
                context: HttpContext
            );
            var singleUserPlan = await singleUserPlanResponse.Content.ReadAsStringAsync();

            if ( singleUserPlanResponse.StatusCode == HttpStatusCode.OK )
            {
                var model = await _loader.GetPage<SetupPage>(id, HttpContext.User, draft);
                model.Email = email;
                model.SelectedPlan = JsonConvert.DeserializeObject<UserPlan>(singleUserPlan);
                if(model.SelectedPlan.Price > 0)
                {
                    model.MollieCheckoutUrl = await GenerateMollieCheckoutUrl(email, model.SelectedPlan, email);
                }
                if (!string.IsNullOrWhiteSpace(code))
                {
                    model.Code = code;
                }

                return View(model);
            }

            return LocalRedirect("/");
        }

        private async Task<string> GenerateMollieCheckoutUrl(string email, UserPlan userPlan, string businessUnitName)
        {
            var customerController = new CustomerController(_api, _clientFactory);
            var customerCreationResponse = await customerController.CreateCustomerAsync(email, businessUnitName);
            int interval = Convert.ToInt32(userPlan.PayInterval);

            var checkoutUrl = await customerController.CreatePaymentRequest(userPlan, email, customerCreationResponse.Value.ToString(), HttpContext);
            if (checkoutUrl != null)
            {
                return checkoutUrl;
            }

            return null;
        }

        [Route("/api/getUserPlans")]
        public async Task<JsonResult> GetUserPlansAsync()
        {
            List<UserPlan> userPlans = await _apiHelper.GetUserPlansAsync();

            return new JsonResult(userPlans);
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
