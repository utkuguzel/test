using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;

namespace RentVision.Models.Regions
{
    public class Plan
    {
        [Field]
        public StringField Title { get; set; }

        [Field]
        public StringField Icon { get; set; }

        [Field]
        public StringField Price { get; set; }
        
        [Field]
        public StringField PaymentType { get; set; }

        [Field]
        public PageField PageLink { get; set; }
    }
}