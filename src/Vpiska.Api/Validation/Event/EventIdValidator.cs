using System;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class EventIdValidator : AbstractValidator<EventIdRequest>
    {
        public EventIdValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty()
                .WithMessage(EventConstants.IdIsEmpty);

            RuleFor(x => x.EventId)
                .Must(id => Guid.TryParse(id, out _))
                .When(x => x.EventId != null)
                .WithMessage(EventConstants.IdIsEmpty);
        }
    }
}