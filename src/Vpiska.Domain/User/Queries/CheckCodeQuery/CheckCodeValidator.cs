using FluentValidation;

namespace Vpiska.Domain.User.Queries.CheckCodeQuery
{
    public sealed class CheckCodeValidator : AbstractValidator<CheckCodeQuery>
    {
        public CheckCodeValidator()
        {
            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.PhoneIsEmpty)
                .Matches(Constants.PhoneRegex)
                .WithErrorCode(Constants.PhoneRegexInvalid);

            RuleFor(x => x.Code)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(Constants.CodeIsEmpty)
                .Must(x => x >= Constants.MinCodeLength && x <= Constants.MaxCodeLength)
                .WithErrorCode(Constants.CodeLengthInvalid);
        }
    }
}