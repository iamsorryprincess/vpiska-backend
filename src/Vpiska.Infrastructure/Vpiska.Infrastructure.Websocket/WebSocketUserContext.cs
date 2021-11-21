namespace Vpiska.Infrastructure.Websocket
{
    public sealed class WebSocketUserContext
    {
        public string UserId { get; }

        public string Username { get; }

        public string UserImageId { get; }

        public WebSocketUserContext(string userId, string username, string userImageId)
        {
            UserId = userId;
            Username = username;
            UserImageId = userImageId;
        }
    }
}