using System;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class RemoveMediaValidator : AbstractValidator<RemoveMediaRequest>
    {
        public RemoveMediaValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty()
                .WithMessage(EventConstants.IdIsEmpty);

            RuleFor(x => x.EventId)
                .Must(id => Guid.TryParse(id, out _))
                .When(x => x.EventId != null)
                .WithMessage(EventConstants.IdIsEmpty);
            
            RuleFor(x => x.MediaId)
                .NotEmpty()
                .WithMessage(EventConstants.MediaIdIsEmpty);

            RuleFor(x => x.MediaId)
                .Must(id => Guid.TryParse(id, out _))
                .When(x => x.MediaId != null)
                .WithMessage(EventConstants.MediaIdIsEmpty);
        }
    }
}