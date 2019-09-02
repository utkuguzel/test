using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;

namespace RentVision.Models
{
    [PageType(Title = "Standard page")]
    public class StandardPage  : Page<StandardPage>
    {
        /// <summary>
        /// Gets/sets the page hero.
        /// </summary>
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }
    }
}