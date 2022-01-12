using System;
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
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage(UserConstants.IdInvalidFormat);

            RuleFor(x => x.Phone)
                .Matches(UserConstants.PhoneRegex)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage(UserConstants.PhoneRegexInvalid);
        }
    }
}