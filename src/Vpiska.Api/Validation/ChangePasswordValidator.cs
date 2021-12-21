using FluentValidation;
using Vpiska.Api.Constants;
using Vpiska.Api.Requests;

namespace Vpiska.Api.Validation
{
    public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(UserConstants.IdIsEmpty);
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(UserConstants.PasswordIsEmpty)
                .Must(x => x.Length >= UserConstants.PasswordLength)
                .When(x => x.Password != null)
                .WithMessage(UserConstants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage(UserConstants.ConfirmPasswordInvalid);
        }
    }
}