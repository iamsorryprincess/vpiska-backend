using FluentValidation;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Requests;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class GetEventsValidator : AbstractValidator<GetEventsRequest>
    {
        public GetEventsValidator()
        {
            RuleFor(x => x.Area).NotEmpty().WithMessage(ValidationErrorConstants.AreaIsEmpty);
        }
    }
}