using System.Text.RegularExpressions;
using FluentValidation;
using Vpiska.Domain.Requests;

namespace Vpiska.Domain.Validation
{
    public sealed class CodeRequestValidator : AbstractValidator<CodeRequest>
    {
        private const string PhonePattern = @"^\d{10}\b$";
        
        public CodeRequestValidator()
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage(ErrorCodes.PhoneIsEmpty)
                .Must(phone => Regex.IsMatch(phone, PhonePattern))
                .WithMessage(ErrorCodes.PhoneRegexNotPassed);
        }
    }
}