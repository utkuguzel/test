using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;
using System.Collections;

namespace RentVision.Models.Regions
{
    public class RegisterFields
    {
        [Field(Title = "E-mail", Options = FieldOption.HalfWidth)]
        public StringField Email { get; set; }

        [Field(Title = "Subdomain", Options = FieldOption.HalfWidth)]
        public StringField Subdomain { get; set; }

        [Field(Title = "Business unit name", Options = FieldOption.HalfWidth)]
        public StringField BusinessUnitName { get; set; }

        [Field(Title = "Password", Options = FieldOption.HalfWidth)]
        public StringField Password { get; set; }

        [Field(Title = "Confirm password", Options = FieldOption.HalfWidth)]
        public StringField ConfirmPassword { get; set; }

        [Field(Title = "Terms of service", Options = FieldOption.HalfWidth)]
        public StringField Tos { get; set; }

        [Field(Title = "Create account", Options = FieldOption.HalfWidth)]
        public StringField ButtonText { get; set; }

        [Field(Title = "Button link", Options = FieldOption.HalfWidth)]
        public StringField ButtonLink { get; set; }

        [Field(Title = "Return to website", Options = FieldOption.HalfWidth)]
        public StringField ReturnButtonText { get; set; }

        // Password requirement strings

        [Field(Options = FieldOption.HalfWidth)]
        public StringField PasswordRequirementText { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public StringField minLengthRequirement { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public StringField numberRequirement { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public StringField upperCaseRequirement { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public StringField specialCharRequirement { get; set; }

        [Field(Options = FieldOption.HalfWidth)]
        public StringField passwordMatchRequirement { get; set; }
    }
}