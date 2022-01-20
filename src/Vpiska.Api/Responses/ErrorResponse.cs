namespace Vpiska.Api.Responses
{
    public sealed class ErrorResponse
    {
        public string ErrorCode { get; }

        public ErrorResponse(string errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}