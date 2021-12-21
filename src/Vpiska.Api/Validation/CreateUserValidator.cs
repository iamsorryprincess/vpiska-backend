using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests;

namespace Vpiska.Api.Validation
{
    public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage(UserConstants.PhoneIsEmpty);
            
            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => x.Phone != null)
                .WithMessage(UserConstants.PhoneRegexInvalid);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(UserConstants.NameIsEmpty);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(UserConstants.PasswordIsEmpty)
                .Must(x => x.Length >= UserConstants.PasswordLength)
                .When(x => x.Password != null)
                .WithMessage(UserConstants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage(UserConstants.ConfirmPasswordInvalid);
        }
    }
}