using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Requests;

namespace Vpiska.Api.Validation.User
{
    internal sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage(ValidationErrorConstants.PhoneIsEmpty);
            
            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => x.Phone != null)
                .WithMessage(ValidationErrorConstants.PhoneRegexInvalid);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationErrorConstants.NameIsEmpty);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationErrorConstants.PasswordIsEmpty)
                .Length(UserConstants.PasswordLength)
                .WithMessage(ValidationErrorConstants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(ValidationErrorConstants.ConfirmPasswordInvalid);
        }
    }
}