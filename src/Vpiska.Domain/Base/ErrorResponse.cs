namespace Vpiska.Domain.Base
{
    public sealed class ErrorResponse
    {
        public string ErrorCode { get; }

        private ErrorResponse(string errorCode)
        {
            ErrorCode = errorCode;
        }

        public static ErrorResponse Create(string errorCode) => new(errorCode);
    }
}