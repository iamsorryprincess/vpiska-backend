namespace Vpiska.Domain.Requests
{
    public sealed class LoginUserRequest
    {
        public string Phone { get; set; }

        public string Password { get; set; }
    }
}