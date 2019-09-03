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

        [Field(Title = "Page link")]
        public PageField PageLink { get; set; }

        [Field(Title = "Is button active?")]
        public CheckBoxField ButtonActive { get; set; }

        public HeaderItem() {
            PageLink = new PageField();
        }
    }
}
