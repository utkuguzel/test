using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Twinvision.Piranha.RentVision.Resources;

namespace RentVision.Models
{
    public class RegisterPageValidator : AbstractValidator<RegisterForm>
    {
        public RegisterPageValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .WithName( x => Strings.Email)
                .WithMessage( x => Strings.NotEmptyValidator)
                .EmailAddress()
                .WithMessage( x => Strings.EmailValidator);
            RuleFor(user => user.Subdomain)
                .NotEmpty()
                .WithName(x => Strings.Subdomain)
                .WithMessage(x => Strings.NotEmptyValidator)
                .MinimumLength(3)
                .WithMessage(x => Strings.MinimumLength_Simple)
                .MaximumLength(12)
                .WithMessage(x => Strings.MaximumLength_Simple);
            RuleFor(user => user.BusinessUnitName)
                .NotEmpty()
                .WithName(x => Strings.BusinessUnitName)
                .WithMessage(x => Strings.NotEmptyValidator);
            RuleFor(user => user.Password)
                .NotEmpty()
                .WithName(x => Strings.Password)
                .WithMessage(x => Strings.NotEmptyValidator);
            RuleFor(user => user.ConfirmPassword)
                .NotEmpty()
                .WithName(x => Strings.ConfirmPassword)
                .WithMessage(x => Strings.NotEmptyValidator);
            RuleFor(user => user.Password)
                .Matches(x => x.ConfirmPassword)
                .WithName(x => Strings.ConfirmPassword)
                .WithMessage(x => Strings.EqualValidator);
        }
    }
}