using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.Event;

namespace Vpiska.Api.Validation.Event
{
    public sealed class GetEventsValidator : AbstractValidator<GetEventsRequest>
    {
        public GetEventsValidator()
        {
            RuleFor(x => x.Range).NotEmpty().WithMessage(EventConstants.RangeIsEmpty);
            RuleFor(x => x.Coordinates).NotEmpty().WithMessage(EventConstants.CoordinatesIsEmpty);

            RuleFor(x => x.Coordinates)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .When(x => x.Coordinates != null)
                .WithMessage(EventConstants.CoordinatesIsEmpty);
        }
    }
}