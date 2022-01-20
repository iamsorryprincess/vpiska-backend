using System;
using FluentValidation;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    internal sealed class AddMediaValidator : AbstractValidator<AddMediaCommand>
    {
        public AddMediaValidator()
        {
            RuleFor(x => x.EventId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(x => Guid.TryParse(x, out _))
                .WithErrorCode(Constants.InvalidIdFormat);

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage(Constants.MediaContentTypeIsEmpty);
            
            RuleFor(x => x.MediaStream)
                .NotEmpty()
                .WithMessage(Constants.MediaIsEmpty);
        }
    }
}