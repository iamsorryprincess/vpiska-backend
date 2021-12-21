using System.Linq;
using FluentValidation.Results;
using Vpiska.Api.Responses;

namespace Vpiska.Api.Extensions
{
    public static class ValidationExtensions
    {
        public static ApiResponse MapToResponse(this ValidationResult validationResult)
        {
            var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
            return ApiResponse.Error(errors);
        }
    }
}