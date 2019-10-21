using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Twinvision.Piranha.RentVision.Resources;

namespace RentVision.Models
{
    public class LoginPageValidator : AbstractValidator<LoginForm>
    {
        public LoginPageValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .WithMessage(x => Strings.EmailInvalid)
                .EmailAddress();
            RuleFor(user => user.Password)
                .NotEmpty();
        }
    }
}