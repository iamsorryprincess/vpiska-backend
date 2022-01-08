namespace Vpiska.Api.Requests.User
{
    public sealed class CheckCodeRequest
    {
        public string Phone { get; set; }

        public int? Code { get; set; }
    }
}