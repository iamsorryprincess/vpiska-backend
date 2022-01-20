namespace Vpiska.Domain.User.Commands.LoginUserCommand
{
    public sealed class LoginUserCommand
    {
        public string Phone { get; set; }

        public string Password { get; set; }
    }
}