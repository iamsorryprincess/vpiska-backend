using System;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketUserContext
    {
        public Guid UserId { get; }

        public string Username { get; }

        public string UserImageId { get; }

        public WebSocketUserContext(Guid userId, string username, string userImageId)
        {
            UserId = userId;
            Username = username;
            UserImageId = userImageId;
        }
    }
}