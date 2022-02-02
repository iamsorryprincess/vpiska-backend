using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketInteracting<TListener> : IWebSocketInteracting<TListener> where TListener : IWebSocketListener
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private readonly WebSocketHub<TListener> _hub;

        public WebSocketInteracting(WebSocketHub<TListener> hub)
        {
            _hub = hub;
        }

        public Task SendMessage<TMessage>(Guid connectionId, string route, TMessage message) where TMessage : class, new()
        {
            var data = Encoding.UTF8.GetBytes($"{route}/{JsonSerializer.Serialize(message, _jsonSerializerOptions)}");
            return _hub.SendMessage(connectionId, data);
        }

        public Task SendRawMessage(Guid connectionId, string route, string message)
        {
            var data = Encoding.UTF8.GetBytes($"{route}/{message}");
            return _hub.SendMessage(connectionId, data);
        }

        public Task Close(Guid connectionId) => _hub.Close(connectionId);
    }
}