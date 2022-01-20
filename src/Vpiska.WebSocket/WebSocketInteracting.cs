using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketInteracting<TConnector, TReceiver> : IWebSocketInteracting<TConnector>
        where TConnector : IWebSocketConnector
        where TReceiver : IWebSocketReceiver
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private readonly WebSocketHub<TConnector, TReceiver> _hub;

        public WebSocketInteracting(WebSocketHub<TConnector, TReceiver> hub)
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