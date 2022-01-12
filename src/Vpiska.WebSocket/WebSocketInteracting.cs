using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketInteracting<TConnector, TReceiver> : IWebSocketInteracting<TConnector>
        where TConnector : IWebSocketConnector
        where TReceiver : IWebSocketReceiver
    {
        private readonly WebSocketHub<TConnector, TReceiver> _hub;

        public WebSocketInteracting(WebSocketHub<TConnector, TReceiver> hub)
        {
            _hub = hub;
        }

        public Task SendMessage(Guid connectionId, byte[] data) => _hub.SendMessage(connectionId, data);

        public Task Close(Guid connectionId) => _hub.Close(connectionId);
    }
}