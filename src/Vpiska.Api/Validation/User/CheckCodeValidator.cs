using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.User;

namespace Vpiska.Api.Validation.User
{
    public sealed class CheckCodeValidator : AbstractValidator<CheckCodeRequest>
    {
        public CheckCodeValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.PhoneIsEmpty)
                .Matches(UserConstants.PhoneRegex)
                .WithMessage(UserConstants.PhoneRegexInvalid);

            RuleFor(x => x.Code)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.CodeIsEmpty)
                .Must(x => x >= UserConstants.MinCodeLength && x <= UserConstants.MaxCodeLength)
                .WithMessage(UserConstants.CodeLengthInvalid);
        }
    }
}