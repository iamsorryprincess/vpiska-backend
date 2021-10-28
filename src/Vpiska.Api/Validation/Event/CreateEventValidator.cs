using FluentValidation;
using Vpiska.Api.Models;
using Vpiska.Domain.EventAggregate.Constants;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class CreateEventValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(ValidationErrorConstants.NameIsEmpty);
            RuleFor(x => x.Coordinates).NotEmpty().WithMessage(ValidationErrorConstants.CoordinatesIsEmpty);
            RuleFor(x => x.Address).NotEmpty().WithMessage(ValidationErrorConstants.AddressIsEmpty);
            RuleFor(x => x.Area).NotEmpty().WithMessage(ValidationErrorConstants.AreaIsEmpty);
        }
    }
}