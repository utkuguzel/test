using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;

namespace RentVision.Models
{
    [PageType(Title = "Start page")]
    [PageTypeRoute(Title = "Default", Route = "/start")]
    public class StartPage : Page<StartPage>
    {
        [Region(Display = RegionDisplayMode.Setting)]
        public Hero Hero { get; set; }

        [Region(Title = "Header", ListTitle = "Title")]
        public IList<HeaderItem> HeaderItems { get; set; }

        [Region(Title = "Footer", ListTitle = "Title")]
        public IList<FooterItem> FooterItems { get; set;}

        public StartPage()
        {
            HeaderItems = new List<HeaderItem>();
            FooterItems = new List<FooterItem>();
        }
    }
}