using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.User;

namespace Vpiska.Api.Validation.User
{
    public sealed class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(UserConstants.IdIsEmpty);

            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage(UserConstants.PhoneRegexInvalid);
        }
    }
}