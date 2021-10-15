using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Domain.Constants;
using Vpiska.Domain.Requests;

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