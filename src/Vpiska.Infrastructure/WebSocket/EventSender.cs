using System;
using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class EventSender : IEventSender
    {
        private readonly IWebSocketInteracting<ChatListener> _webSocketInteracting;

        public EventSender(IWebSocketInteracting<ChatListener> webSocketInteracting)
        {
            _webSocketInteracting = webSocketInteracting;
        }

        public Task SendUsersCountUpdate(Guid[] connections, int usersCount)
        {
            var message = usersCount.ToString();
            return Task.WhenAll(connections.Select(connectionId =>
                _webSocketInteracting.SendRawMessage(connectionId, "usersCountUpdated", message)));
        }

        public Task SendChatMessageToConnections(Guid[] connectionIds, ChatMessage chatMessage) =>
            Task.WhenAll(connectionIds.Select(connectionId =>
                _webSocketInteracting.SendMessage(connectionId, "chatMessage", chatMessage)));

        public async Task SendCloseStatus(Guid connectionId)
        {
            await _webSocketInteracting.SendRawMessage(connectionId, "closeEvent", string.Empty);
            await _webSocketInteracting.Close(connectionId);
        }

        public Task NotifyMediaAdded(Guid[] connections, string mediaId) =>
            Task.WhenAll(connections.Select(connectionId =>
                _webSocketInteracting.SendRawMessage(connectionId, "mediaAdded", mediaId)));

        public Task NotifyMediaRemoved(Guid[] connections, string mediaId) =>
            Task.WhenAll(connections.Select(connectionId =>
                _webSocketInteracting.SendRawMessage(connectionId, "mediaRemoved", mediaId)));
    }
}