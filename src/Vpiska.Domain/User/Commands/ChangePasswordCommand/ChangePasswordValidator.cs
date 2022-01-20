using System;
using FluentValidation;

namespace Vpiska.Domain.User.Commands.ChangePasswordCommand
{
    internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithErrorCode(Constants.InvalidIdFormat);
            
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PasswordIsEmpty)
                .Must(x => x.Length >= Constants.PasswordLength)
                .WithErrorCode(Constants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithErrorCode(Constants.ConfirmPasswordInvalid);
        }
    }
}