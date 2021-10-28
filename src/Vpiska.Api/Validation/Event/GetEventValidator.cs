using FluentValidation;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Requests;

namespace Vpiska.Api.Validation.Event
{
    internal sealed class GetEventValidator : AbstractValidator<GetEventRequest>
    {
        public GetEventValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(ValidationErrorConstants.EventIdIsEmpty);
        }
    }
}