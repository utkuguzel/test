using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Verification page")]
    [PageTypeRoute(Title = "Default", Route = "/verify")]
    public class VerificationPage : Page<VerificationPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }

        [Region]
        public VerificationFields Fields { get; set; }

        public string Email { get; set; }
    }
}