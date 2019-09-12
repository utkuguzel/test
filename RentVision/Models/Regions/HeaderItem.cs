using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace RentVision.Models.Regions
{
    public class HeaderItem
    {
        [Field]
        public StringField Icon { get; set; }

        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField PageLinkFixed { get; set; }
    }
}
