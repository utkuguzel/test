using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using System.Collections;

namespace RentVision.Models.Regions
{
    public class VerificationFields
    {
        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField Content { get; set; }

        [Field]
        public StringField Continue { get; set; }
    }
}