using FluentValidation;
using Vpiska.Api.Models;
using Vpiska.Domain.EventAggregate.Constants;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class CloseEventValidator : AbstractValidator<CloseEventRequest>
    {
        public CloseEventValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage(ValidationErrorConstants.EventIdIsEmpty);
        }
    }
}