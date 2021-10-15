namespace Vpiska.Domain.Responses
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