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
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(EventConstants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage(EventConstants.InvalidIdFormat);

            RuleFor(x => x.MediaId)
                .NotEmpty()
                .WithMessage(EventConstants.MediaIdIsEmpty);
        }
    }
}