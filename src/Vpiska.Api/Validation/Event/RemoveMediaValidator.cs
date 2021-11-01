using FluentValidation;
using Vpiska.Api.Models;
using Vpiska.Domain.EventAggregate.Constants;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class RemoveMediaValidator : AbstractValidator<RemoveMediaRequest>
    {
        public RemoveMediaValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage(ValidationErrorConstants.EventIdIsEmpty);
            RuleFor(x => x.MediaLink).NotEmpty().WithMessage(ValidationErrorConstants.MediaLinkIsEmpty);
        }
    }
}