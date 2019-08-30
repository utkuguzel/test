using Piranha.AttributeBuilder;
using Piranha.Extend.Fields;
using Piranha.Models;
using System.Collections;

namespace RentVision.Models.Regions
{
    public class RegisterFields
    {
        [Field(Title = "Account type", Options = FieldOption.HalfWidth)]
        public StringField AccountTypeText { get; set; }

        [Field(Title = "Firstname", Options = FieldOption.HalfWidth)]
        public StringField FirstName { get; set; }

        [Field(Title = "Lastname", Options = FieldOption.HalfWidth)]
        public StringField LastName { get; set; }

        [Field(Title = "E-mail", Options = FieldOption.HalfWidth)]
        public StringField Email { get; set; }

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
    }
}