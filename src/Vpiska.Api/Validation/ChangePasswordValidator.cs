using FluentValidation;
using Vpiska.Domain.Constants;
using Vpiska.Domain.Requests;

namespace Vpiska.Api.Validation
{
    internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(ValidationErrorConstants.IdIsEmpty);
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationErrorConstants.PasswordIsEmpty)
                .Length(UserConstants.PasswordLength)
                .WithMessage(ValidationErrorConstants.PasswordLengthInvalid);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(ValidationErrorConstants.ConfirmPasswordInvalid);
        }
    }
}