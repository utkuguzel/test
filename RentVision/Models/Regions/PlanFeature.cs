using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using System.Collections;

namespace RentVision.Models.Regions
{
    public class PlanFeature
    {
        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField Icon { get; set; }

        [Field]
        public CheckBoxField PlanFreeEnabled { get; set; }

        [Field]
        public CheckBoxField PlanBasicEnabled { get; set; }

        [Field]
        public CheckBoxField PlanProEnabled { get; set; }
    }
}