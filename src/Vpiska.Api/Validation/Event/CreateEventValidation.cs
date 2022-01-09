using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class CreateEventValidation : AbstractValidator<CreateEventRequest>
    {
        public CreateEventValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(EventConstants.NameIsEmpty);

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage(EventConstants.AddressIsEmpty);

            RuleFor(x => x.Coordinates)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(EventConstants.CoordinatesAreEmpty)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .WithMessage(EventConstants.CoordinatesAreEmpty);
        }
    }
}