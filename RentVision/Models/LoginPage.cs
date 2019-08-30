using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Login page")]
    [PageTypeRoute(Title = "Default", Route = "/login")]
    public class LoginPage : Page<LoginPage>
    {
        [Region]
        public LoginFields Fields { get; set; }
    }
}