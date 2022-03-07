using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    public sealed class CloseEventValidator : AbstractValidator<CloseEventCommand>
    {
        public CloseEventValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);
            
            RuleFor(x => x.OwnerId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);
        }
    }
}