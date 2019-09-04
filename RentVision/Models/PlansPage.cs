using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Plans page")]
    [PageTypeRoute(Title = "Default", Route = "/plans")]
    public class PlansPage  : Page<PlansPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }

        [Region(ListTitle = "Title")]
        public IList<Plan> Plans { get; set; }

        public PlansPage()
        {
            Plans = new List<Plan>();
        }
    }
}