using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    public sealed class RemoveMediaValidator : AbstractValidator<RemoveMediaCommand>
    {
        public RemoveMediaValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithErrorCode(Constants.InvalidIdFormat);

            RuleFor(x => x.MediaId)
                .NotEmpty()
                .WithErrorCode(Constants.MediaIdIsEmpty);
        }
    }
}