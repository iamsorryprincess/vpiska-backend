using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketSender<TConnector, TReceiver> : IWebSocketSender<TConnector>
        where TConnector : IWebSocketConnector
        where TReceiver : IWebSocketReceiver
    {
        private readonly WebSocketHub<TConnector, TReceiver> _hub;

        public WebSocketSender(WebSocketHub<TConnector, TReceiver> hub)
        {
            _hub = hub;
        }

        public Task SendMessage(Guid connectionId, byte[] data) => _hub.SendMessage(connectionId, data);
    }
}