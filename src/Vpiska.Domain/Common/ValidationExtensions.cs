using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.Domain.Common
{
    public static class ValidationExtensions
    {
        public static async Task ValidateRequest<TRequest>(this IValidator<TRequest> validator, TRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorCodes = validationResult.Errors.Select(failure => failure.ErrorCode).ToArray();
                throw new ValidationException(errorCodes);
            }
        }
    }
}