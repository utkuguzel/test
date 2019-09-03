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

        [Region]
        public IList<HeaderItem> HeaderItems { get; set; }

        [Region]
        public IList<FooterPage> FooterPages { get; set;}

        public StartPage()
        {
            HeaderItems = new List<HeaderItem>();
            FooterPages = new List<FooterPage>();
        }
    }
}