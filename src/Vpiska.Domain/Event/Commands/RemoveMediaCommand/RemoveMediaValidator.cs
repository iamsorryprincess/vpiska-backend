using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    internal sealed class RemoveMediaValidator : AbstractValidator<RemoveMediaCommand>
    {
        public RemoveMediaValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Constants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage(Constants.InvalidIdFormat);

            RuleFor(x => x.MediaId)
                .NotEmpty()
                .WithMessage(Constants.MediaIdIsEmpty);
        }
    }
}