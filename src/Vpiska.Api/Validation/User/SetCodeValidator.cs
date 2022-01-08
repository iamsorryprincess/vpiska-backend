using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.User;

namespace Vpiska.Api.Validation.User
{
    public sealed class SetCodeValidator : AbstractValidator<SetCodeRequest>
    {
        public SetCodeValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.PhoneIsEmpty)
                .Matches(UserConstants.PhoneRegex)
                .WithMessage(UserConstants.PhoneRegexInvalid);

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage(UserConstants.TokenIsEmpty);
        }
    }
}