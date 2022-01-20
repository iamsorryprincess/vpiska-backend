namespace Vpiska.Domain.User.Commands.CreateUserCommand
{
    public sealed class CreateUserCommand
    {
        public string Phone { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}