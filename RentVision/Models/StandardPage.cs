using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;

namespace RentVision.Models
{
    [PageType(Title = "Standard page")]
    [PageTypeRoute(Title = "Default")]
    public class StandardPage  : Page<StandardPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }
    }
}