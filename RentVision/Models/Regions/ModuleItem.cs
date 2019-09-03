using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;

namespace RentVision.Models.Regions
{
    public class ModuleItem
    {
        [Field( Title = "Module icon" )]
        public StringField Icon { get; set; }

        [Field( Title = "Module name" )]
        public StringField Name { get; set; }

        [Field( Title = "Module page link" )]
        public PageField Page { get; set; }
    }
}