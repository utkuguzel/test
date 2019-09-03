using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace RentVision.Models.Regions
{
    public class FooterItem
    {
        [Field(Options = FieldOption.HalfWidth)]
        public StringField Title { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public NumberField Column { get; set; }

        [Field(Title = "Page link")]
        public PageField PageLink { get; set; }

        public FooterItem() {
            PageLink = new PageField();
        }
    }
}
