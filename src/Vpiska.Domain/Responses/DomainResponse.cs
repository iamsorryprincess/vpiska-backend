using System.Collections.Generic;

namespace Vpiska.Domain.Responses
{
    public class DomainResponse<TResult>
    {
        public bool IsSuccess { get; }

        public List<ErrorResponse> Errors { get; }

        public TResult Result { get; }

        protected DomainResponse(List<ErrorResponse> errors)
        {
            IsSuccess = false;
            Errors = errors ?? new List<ErrorResponse>();
        }

        protected DomainResponse(TResult result)
        {
            IsSuccess = true;
            Result = result;
        }

        public static DomainResponse<TResult> CreateError(List<ErrorResponse> errors = null) => new(errors);

        public static DomainResponse<TResult> CreateSuccess(TResult result) => new(result);
    }

    public sealed class DomainResponse : DomainResponse<string>
    {
        private DomainResponse(List<ErrorResponse> errors) : base(errors)
        {
        }

        private DomainResponse(string result = null) : base(result)
        {
        }
        
        public new static DomainResponse CreateError(List<ErrorResponse> errors = null) => new(errors);

        public static DomainResponse CreateSuccess() => new();
    }
}