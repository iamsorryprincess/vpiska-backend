namespace Vpiska.Domain.User.Commands.SetCodeCommand
{
    public sealed class SetCodeCommand
    {
        public string Phone { get; set; }

        public string Token { get; set; }
    }
}