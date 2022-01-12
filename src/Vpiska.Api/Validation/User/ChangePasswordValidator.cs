using System;
using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests.User;

namespace Vpiska.Api.Validation.User
{
    public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.IdIsEmpty)
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage(UserConstants.IdInvalidFormat);
            
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(UserConstants.PasswordIsEmpty)
                .Must(x => x.Length >= UserConstants.PasswordLength)
                .WithMessage(UserConstants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage(UserConstants.ConfirmPasswordInvalid);
        }
    }
}