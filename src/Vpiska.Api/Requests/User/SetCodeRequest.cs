namespace Vpiska.Api.Requests.User
{
    public sealed class SetCodeRequest
    {
        public string Phone { get; set; }

        public string Token { get; set; }
    }
}