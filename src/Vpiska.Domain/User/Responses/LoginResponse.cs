namespace Vpiska.Domain.User.Responses
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; }
        
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string ImageId { get; set; }
    }
}