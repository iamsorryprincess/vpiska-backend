using System;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class IdValidator : AbstractValidator<IdRequest>
    {
        public IdValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(EventConstants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage(EventConstants.InvalidIdFormat);
        }
    }
}