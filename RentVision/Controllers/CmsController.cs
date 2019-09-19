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

            TempData["UserPlan"] = userPlan;
            TempData["PayInterval"] = payInterval;
            TempData["StatusMessage"] = "";

            // Return user to proper culture page
            string cultureUrl = CultureHelper.GetProperCultureUrl(Request, HttpContext);
            
            if ( cultureUrl != null )
            {
                return LocalRedirect(cultureUrl);
            }

            return View(model);
        }

        [Route("product")]
        public async Task<IActionResult> Product(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<ProductPage>(id, HttpContext.User, draft);

            return View(model);
        }

        [Route("plans")]
        public async Task<IActionResult> Plans(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<PlansPage>(id, HttpContext.User, draft);

            //List<UserPlan> userPlans = await _apiHelper.GetUserPlansAsync();
            //model.UserPlans = userPlans;

            return View(model);
        }

        [Route("setup")]
        public async Task<IActionResult> Setup(Guid id, bool draft = false)
        {
            var model = await _loader.GetPage<SetupPage>(id, HttpContext.User, draft);

            // Set temporary debug data if _DEBUG is enabled
            if ( ( TempData["Email"] == null ) && !_DEBUG )
            {
                return LocalRedirect("/");
            }
            else if ( _DEBUG )
            {
                TempData["Email"] = "";
            }

            return View(model);
        }

        [Route("/api/getUserPlans")]
        public async Task<JsonResult> GetUserPlansAsync()
        {
            List<UserPlan> userPlans = await _apiHelper.GetUserPlansAsync();

            return new JsonResult(userPlans);
        }
    }
}
