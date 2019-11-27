using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RentVision.Models
{
    [PageType(Title = "Register page")]
    [PageTypeRoute(Title = "Default", Route = "/auth/register")]
    public class RegisterPage : Page<RegisterPage>
    {
        [Region]
        public RegisterFields Fields { get; set; }

        public Controllers.Plan SelectedUserPlan { get; set; }

        [BindProperty, Required]
        public string Email { get; set; }

        [BindProperty, Required]
        public string Subdomain { get; set; }

        [BindProperty, Required]
        public string BusinessUnitName { get; set; }

        [BindProperty, Required, Compare("ConfirmPassword")]
        public string Password { get; set; }

        [BindProperty, Required, Compare("Password")]
        public string ConfirmPassword { get; set; }

        [BindProperty, Required]
        public bool Tos { get; set; }
    }
}