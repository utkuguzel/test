using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RentVision.Models
{
    public class RegisterForm
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Subdomain { get; set; }

        [BindProperty]
        public string BusinessUnitName { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public bool Tos { get; set; }
    }
}