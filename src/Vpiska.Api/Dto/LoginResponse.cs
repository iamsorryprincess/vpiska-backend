namespace Vpiska.Api.Dto
{
    public sealed class LoginResponse
    {
        public string UserId { get; set; }
        
        public string AccessToken { get; set; }
    }
}