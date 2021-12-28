using System.Text.RegularExpressions;
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
                .NotEmpty()
                .WithMessage(UserConstants.PhoneIsEmpty);
            
            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => x.Phone != null)
                .WithMessage(UserConstants.PhoneRegexInvalid);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(UserConstants.CodeIsEmpty)
                .Must(x => x >= UserConstants.MinCodeLength && x <= UserConstants.MaxCodeLength)
                .WithMessage(UserConstants.CodeLengthInvalid);
        }
    }
}