using FluentValidation;

namespace Vpiska.Domain.User.Commands.LoginUserCommand
{
    public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PhoneIsEmpty)
                .Matches(Constants.PhoneRegex)
                .WithErrorCode(Constants.PhoneRegexInvalid);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PasswordIsEmpty)
                .Must(x => x.Length >= Constants.PasswordLength)
                .WithErrorCode(Constants.PasswordLengthInvalid);
        }
    }
}