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
    }
}