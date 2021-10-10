using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Domain.Requests;

namespace Vpiska.Domain.Validation
{
    public sealed class LoginUserValidator : AbstractValidator<LoginRequest>
    {
        private const string PhonePattern = @"^\d{10}\b$";
        
        public LoginUserValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage(ErrorCodes.PhoneIsEmpty)
                .Must(phone => Regex.IsMatch(phone, PhonePattern))
                .WithMessage(ErrorCodes.PhoneRegexNotPassed);
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ErrorCodes.PasswordIsEmpty)
                .Length(6)
                .WithMessage(ErrorCodes.PasswordLengthInvalid);
        }
    }
}