using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using System.Collections;

namespace RentVision.Models.Regions
{
    public enum PlanTypes
    {
        Free,
        Basic,
        Pro
    }

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

        [Field]
        public SelectField<PlanTypes> PlanType { get; set; }
    }
}