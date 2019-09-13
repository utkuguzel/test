using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;

namespace RentVision.Models.Regions
{
    public class Hero
    {
        /// <summary>
        /// Gets/sets the optional primary image.
        /// </summary>
        [Field(Title = "Primary image")]
        public ImageField PrimaryImage { get; set; }

        /// <summary>
        /// Gets/sets the optional primary image.
        /// </summary>
        [Field(Title = "Secondary image")]
        public ImageField SecondaryImage { get; set; }

        /// <summary>
        /// Gets/sets the optional title.
        /// </summary>
        [Field]
        public TextField Title { get; set; }

        /// <summary>
        /// Gets/sets the optional ingress.
        /// </summary>
        [Field]
        public TextField Ingress { get; set; }
    }
}