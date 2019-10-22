using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using RentVision.Helpers;
using Twinvision.Piranha.RentVision.Resources;

namespace RentVision.Models
{
    public class LoginPageValidator : AbstractValidator<LoginForm>
    {
        public LoginPageValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .WithName(x => Strings.Email)
                .WithMessage(x => Strings.NotEmptyValidator)
                .EmailAddress()
                .WithMessage(x => Strings.EmailValidator);
            RuleFor(user => user.Password)
                .NotEmpty()
                .WithName( x => Strings.Password )
                .WithMessage(x => Strings.NotEmptyValidator);
        }
    }
}