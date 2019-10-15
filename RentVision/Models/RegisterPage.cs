using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Register page")]
    [PageTypeRoute(Title = "Default", Route = "/register")]
    public class RegisterPage : Page<RegisterPage>
    {
        [Region]
        public RegisterFields Fields { get; set; }

        public UserPlan SelectedUserPlan { get; set; }
    }
}