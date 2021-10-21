using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Api.Models;
using Vpiska.Domain.UserAggregate.Constants;

namespace Vpiska.Api.Validation
{
    internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationErrorConstants.IdIsEmpty);

            RuleFor(x => x.Phone)
                .Must(x => Regex.IsMatch(x, UserConstants.PhoneRegex))
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage(ValidationErrorConstants.PhoneRegexInvalid);
        }
    }
}