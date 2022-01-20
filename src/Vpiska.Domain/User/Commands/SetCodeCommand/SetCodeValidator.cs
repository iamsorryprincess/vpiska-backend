using FluentValidation;

namespace Vpiska.Domain.User.Commands.SetCodeCommand
{
    internal sealed class SetCodeValidator : AbstractValidator<SetCodeCommand>
    {
        public SetCodeValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PhoneIsEmpty)
                .Matches(Constants.PhoneRegex)
                .WithErrorCode(Constants.PhoneRegexInvalid);

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithErrorCode(Constants.TokenIsEmpty);
        }
    }
}