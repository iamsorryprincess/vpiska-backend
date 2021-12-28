using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class CreateEventValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(EventConstants.NameIsEmpty);
            RuleFor(x => x.Address).NotEmpty().WithMessage(EventConstants.AddressIsEmpty);
            RuleFor(x => x.Coordinates).NotEmpty().WithMessage(EventConstants.CoordinatesIsEmpty);

            RuleFor(x => x.Coordinates)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .When(x => x.Coordinates != null)
                .WithMessage(EventConstants.CoordinatesIsEmpty);
        }
    }
}