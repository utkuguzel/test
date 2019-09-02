using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace RentVision.Models.Regions
{
    /// <summary>
    /// Simple region for a teaser.
    /// </summary>
    public class FooterPage
    {
        /// <summary>
        /// Gets/sets the main title.
        /// </summary>
        [Field(Options = FieldOption.HalfWidth)]
        public StringField Title { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public NumberField Column { get; set; }

        /// <summary>
        /// Gets/sets the optional page link.
        /// </summary>
        [Field(Title = "Page link")]
        public PageField PageLink { get; set; }

        public FooterPage() {
            PageLink = new PageField();
        }
    }
}
