namespace Vpiska.Api.Requests
{
    public sealed class LoginUserRequest
    {
        public string Phone { get; set; }

        public string Password { get; set; }
    }
}