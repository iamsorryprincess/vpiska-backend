using FluentValidation;

namespace Vpiska.Domain.Event.Queries.GetEventsQuery
{
    public sealed class GetEventsValidator : AbstractValidator<GetEventsQuery>
    {
        public GetEventsValidator()
        {
            RuleFor(x => x.HorizontalRange)
                .NotNull()
                .WithErrorCode(Constants.HorizontalRangeIsEmpty);

            RuleFor(x => x.VerticalRange)
                .NotNull()
                .WithErrorCode(Constants.VerticalRangeIsEmpty);
            
            RuleFor(x => x.Coordinates)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(Constants.CoordinatesAreEmpty)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .WithErrorCode(Constants.CoordinatesAreEmpty);
        }
    }
}