namespace Vpiska.Api.Requests.User
{
    public sealed class CreateUserRequest
    {
        public string Phone { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}