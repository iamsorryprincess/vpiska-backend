using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Queries.GetByIdQuery
{
    public sealed class GetByIdValidator : AbstractValidator<GetByIdQuery>
    {
        public GetByIdValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);
        }
    }
}