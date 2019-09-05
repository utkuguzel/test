using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using System.Collections;

namespace RentVision.Models.Regions
{
    public class SetupFields
    {
        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField Icon { get; set; }

        [Field]
        public TextField LoadingText { get; set; }

        [Field]
        public StringField TimeOutMessage { get; set; }
    }
}