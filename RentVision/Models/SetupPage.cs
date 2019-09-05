using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;

namespace RentVision.Models
{
    [PageType(Title = "Setup page")]
    [PageTypeRoute(Title = "Default", Route = "/setup")]
    public class SetupPage  : Page<SetupPage>
    {
        [Region( Title = "Setup fields" )]
        public SetupFields SetupFields { get; set; }
    }
}