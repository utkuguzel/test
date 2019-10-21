using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace RentVision.Models
{
    [PageType(Title = "Login page", UseBlocks = false)]
    [PageTypeRoute(Title = "Default", Route = "/auth/login")]
    public class LoginPage : Page<LoginPage>
    {
        [Region]
        public LoginFields Fields { get; set; }

        [BindProperty, Required]
        public string Email { get; set; }

        [BindProperty, Required]
        public string Password { get; set; }

        [BindProperty]
        public bool Remember { get; set; }
    }
}