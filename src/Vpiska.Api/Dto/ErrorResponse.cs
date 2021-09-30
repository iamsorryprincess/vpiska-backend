namespace Vpiska.Api.Dto
{
    public sealed class ErrorResponse
    {
        public string Error { get; set; }

        public static ErrorResponse Create(string error) => new ErrorResponse() { Error = error };
    }
}