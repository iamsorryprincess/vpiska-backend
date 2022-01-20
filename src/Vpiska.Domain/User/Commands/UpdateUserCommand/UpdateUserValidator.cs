using System;
using FluentValidation;

namespace Vpiska.Domain.User.Commands.UpdateUserCommand
{
    internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);

            RuleFor(x => x.Phone)
                .Matches(Constants.PhoneRegex)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithErrorCode(Constants.PhoneRegexInvalid);
        }
    }
}