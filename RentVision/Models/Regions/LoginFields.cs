using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;

namespace RentVision.Models.Regions
{
    public class LoginFields
    {
        [Field(Title = "E-mail", Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField Email { get; set; }

        [Field(Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField Password { get; set; }

        [Field(Title = "Remember me", Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField Remember { get; set; }

        [Field(Title = "Sign in", Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField ButtonText { get; set; }

        [Field(Title = "Button link", Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField ButtonLink { get; set; }

        [Field(Title = "Return to website", Options = Piranha.Models.FieldOption.HalfWidth)]
        public StringField ReturnButtonText { get; set; }
    }
}