using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Domain.Constants;
using Vpiska.Domain.Requests;

namespace Vpiska.Api.Validation
{
    internal sealed class LoginUserValidator : AbstractValidator<LoginUserRequest>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage(ValidationErrorConstants.PhoneIsEmpty);
            
            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => x.Phone != null)
                .WithMessage(ValidationErrorConstants.PhoneRegexInvalid);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationErrorConstants.PasswordIsEmpty)
                .Length(UserConstants.PasswordLength)
                .WithMessage(ValidationErrorConstants.PasswordLengthInvalid);
        }
    }
}