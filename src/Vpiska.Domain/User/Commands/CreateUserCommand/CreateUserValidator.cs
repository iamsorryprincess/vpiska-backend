using FluentValidation;

namespace Vpiska.Domain.User.Commands.CreateUserCommand
{
    internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PhoneIsEmpty)
                .Matches(Constants.PhoneRegex)
                .WithErrorCode(Constants.PhoneRegexInvalid);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(Constants.NameIsEmpty);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PasswordIsEmpty)
                .Must(x => x.Length >= Constants.PasswordLength)
                .WithErrorCode(Constants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithErrorCode(Constants.ConfirmPasswordInvalid);
        }
    }
}