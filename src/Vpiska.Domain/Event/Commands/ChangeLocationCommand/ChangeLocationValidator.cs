using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.ChangeLocationCommand
{
    public sealed class ChangeLocationValidator : AbstractValidator<ChangeLocationCommand>
    {
        public ChangeLocationValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithErrorCode(Constants.AddressIsEmpty);
            
            RuleFor(x => x.Coordinates)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithErrorCode(Constants.CoordinatesAreEmpty)
                .Must(x => x.X.HasValue && x.Y.HasValue)
                .WithErrorCode(Constants.CoordinatesAreEmpty);
        }
    }
}