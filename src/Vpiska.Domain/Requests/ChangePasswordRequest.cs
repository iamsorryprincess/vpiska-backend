namespace Vpiska.Domain.Requests
{
    public sealed class ChangePasswordRequest
    {
        public string Id { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
}