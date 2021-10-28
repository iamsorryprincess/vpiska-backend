using FluentValidation;
using Vpiska.Api.Models;
using Vpiska.Domain.EventAggregate.Constants;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class AddMediaValidator : AbstractValidator<AddMediaRequest>
    {
        public AddMediaValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage(ValidationErrorConstants.EventIdIsEmpty);
            RuleFor(x => x.Media).NotEmpty().WithMessage(ValidationErrorConstants.MediaContentIsEmpty);
        }
    }
}