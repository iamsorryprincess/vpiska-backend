namespace Vpiska.Domain.Requests
{
    public sealed class LoginRequest
    {
        public string Phone { get; set; }
        
        public string Password { get; set; }
    }
}