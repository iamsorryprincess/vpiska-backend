namespace Vpiska.Domain.User.Commands.ChangePasswordCommand
{
    public sealed class ChangePasswordCommand
    {
        public string Id { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}