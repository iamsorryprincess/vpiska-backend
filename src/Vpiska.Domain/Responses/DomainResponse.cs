using System;

namespace Vpiska.Domain.Responses
{
    public sealed class DomainResponse<TResponse> where TResponse : class
    {
        public bool IsSuccess { get; }
        
        public TResponse Result { get; }

        public ErrorResponse[] Errors { get; }
        
        private DomainResponse(TResponse result)
        {
            IsSuccess = true;
            Result = result;
            Errors = Array.Empty<ErrorResponse>();
        }

        private DomainResponse(ErrorResponse[] errors)
        {
            IsSuccess = false;
            Result = null;
            Errors = errors;
        }
        
        public static DomainResponse<TResponse> CreateSuccess(TResponse response) => new(response);

        public static DomainResponse<TResponse> CreateError(ErrorResponse[] errors) => new(errors);
    }
    
    public sealed class DomainResponse
    {
        public bool IsSuccess { get; }
        
        public string Result { get; }

        public ErrorResponse[] Errors { get; }
        
        private DomainResponse()
        {
            IsSuccess = true;
            Result = null;
            Errors = Array.Empty<ErrorResponse>();
        }

        private DomainResponse(ErrorResponse[] errors)
        {
            IsSuccess = false;
            Result = null;
            Errors = errors;
        }
        
        public static DomainResponse CreateSuccess() => new();

        public static DomainResponse CreateError(ErrorResponse[] errors) => new(errors);
    }
}