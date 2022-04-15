namespace Vpiska.Domain.User.Responses
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; }
        
        public string UserId { get; }

        public string UserName { get; }

        public string Phone { get; }

        public string ImageId { get; }

        public string EventId { get; }

        public LoginResponse(string token, string eventId, User user)
        {
            AccessToken = token;
            UserId = user.Id;
            UserName = user.Name;
            Phone = user.Phone;
            ImageId = user.ImageId;
            EventId = eventId;
        }
    }
}