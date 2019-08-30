using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace RentVision.Models.Regions
{
    public class AccountTypes
    {
        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField Value { get; set; }

        [Field]
        public CheckBoxField Visible { get; set; } = true;
    }
}