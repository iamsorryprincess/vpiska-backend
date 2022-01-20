using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.CreateEventCommand
{
    internal sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.OwnerId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(Constants.NameIsEmpty);

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