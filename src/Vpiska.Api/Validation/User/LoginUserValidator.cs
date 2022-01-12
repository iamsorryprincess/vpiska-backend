using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.User;

namespace Vpiska.Api.Validation.User
{
    public sealed class LoginUserValidator : AbstractValidator<LoginUserRequest>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.PhoneIsEmpty)
                .Matches(UserConstants.PhoneRegex)
                .WithMessage(UserConstants.PhoneRegexInvalid);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.PasswordIsEmpty)
                .Must(x => x.Length >= UserConstants.PasswordLength)
                .WithMessage(UserConstants.PasswordLengthInvalid);
        }
    }
}