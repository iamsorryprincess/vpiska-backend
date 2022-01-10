using System;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class AddMediaValidator : AbstractValidator<AddMediaRequest>
    {
        public AddMediaValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(EventConstants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage(EventConstants.InvalidIdFormat);

            RuleFor(x => x.Media)
                .NotEmpty()
                .WithMessage(EventConstants.MediaIsEmpty);
        }
    }
}