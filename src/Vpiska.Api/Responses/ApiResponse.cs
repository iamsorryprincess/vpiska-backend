using System;
using System.Linq;

namespace Vpiska.Api.Responses
{
    public sealed class ApiResponse<TResult>
    {
        public bool IsSuccess { get; }

        public TResult Result { get; }

        public ErrorResponse[] Errors { get; }
        
        private ApiResponse(TResult result)
        {
            IsSuccess = true;
            Result = result;
            Errors = Array.Empty<ErrorResponse>();
        }

        private ApiResponse(string[] errors)
        {
            IsSuccess = false;
            Errors = errors.Select(ErrorResponse.Create).ToArray();
        }

        public static ApiResponse<TResult> Success(TResult result) => new(result);

        public static ApiResponse<TResult> Error(params string[] errors) => new(errors);
    }
    
    public sealed class ApiResponse
    {
        public bool IsSuccess { get; }

        public string Result { get; }

        public ErrorResponse[] Errors { get; }
        
        private ApiResponse()
        {
            IsSuccess = true;
            Result = null;
            Errors = Array.Empty<ErrorResponse>();
        }

        private ApiResponse(string[] errors)
        {
            IsSuccess = false;
            Errors = errors.Select(ErrorResponse.Create).ToArray();
        }

        public static ApiResponse Success() => new();

        public static ApiResponse Error(params string[] errors) => new(errors);
    }
}