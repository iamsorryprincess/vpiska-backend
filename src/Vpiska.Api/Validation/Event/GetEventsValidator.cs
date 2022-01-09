using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class GetEventsValidator : AbstractValidator<GetEventsRequest>
    {
        public GetEventsValidator()
        {
            RuleFor(x => x.HorizontalRange)
                .NotNull()
                .WithMessage(EventConstants.HorizontalRangeIsEmpty);

            RuleFor(x => x.VerticalRange)
                .NotNull()
                .WithMessage(EventConstants.VerticalRangeIsEmpty);
            
            RuleFor(x => x.Coordinates)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(EventConstants.CoordinatesAreEmpty)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .WithMessage(EventConstants.CoordinatesAreEmpty);
        }
    }
}