using RentVision.Models;
using RentVision.Models.Regions;
using Microsoft.AspNetCore.Mvc;
using Piranha;
using Piranha.Extend.Blocks;
using System;
using System.Threading.Tasks;
using System.Net;

namespace RentVision.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApi _api;

        public AuthController(IApi api)
        {
            _api = api;
        }

        [Route("/auth/login")]
        public IActionResult Login()
        {
            return new JsonResult( HttpStatusCode.OK );
        }

        [Route("/auth/register")]
        public IActionResult Register()
        {
            return new JsonResult(HttpStatusCode.OK);
        }
    }
}
