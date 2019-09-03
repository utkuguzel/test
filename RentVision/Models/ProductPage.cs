using Piranha.AttributeBuilder;
using Piranha.Models;
using RentVision.Models.Regions;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Product page")]
    [PageTypeRoute(Title = "Default", Route = "/product")]
    public class ProductPage  : Page<ProductPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }

        [Region( Title = "Modules", ListTitle = "Name" )]
        public IList<ModuleItem> ModuleItems { get; set; }

        public ProductPage()
        {
            ModuleItems = new List<ModuleItem>();
        }
    }
}