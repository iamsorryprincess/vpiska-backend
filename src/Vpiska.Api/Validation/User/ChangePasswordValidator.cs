using FluentValidation;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Requests;

namespace Vpiska.Api.Validation.User
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