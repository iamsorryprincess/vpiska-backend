namespace Vpiska.Domain.Requests
{
    public sealed class CheckCodeRequest
    {
        public string Phone { get; set; }

        public int? Code { get; set; }
    }
}