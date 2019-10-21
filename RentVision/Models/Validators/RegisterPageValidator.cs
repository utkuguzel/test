using RentVision.Models.Regions;
using Piranha.AttributeBuilder;
using Piranha.Models;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace RentVision.Models
{
    public class RegisterPageValidator : AbstractValidator<RegisterForm>
    {
        public RegisterPageValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(user => user.Subdomain)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(12);
            RuleFor(user => user.BusinessUnitName)
                .NotEmpty();
            RuleFor(user => user.Password)
                .NotEmpty();
            RuleFor(user => user.ConfirmPassword)
                .NotEmpty();
            RuleFor(user => user.Password)
                .Matches(x => x.ConfirmPassword);
        }
    }
}