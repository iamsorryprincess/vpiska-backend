using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    internal sealed class WebSocketContext
    {
        private readonly WebSocket _socket;
        
        public Dictionary<string, string> QueryParams { get; }

        public WebSocketContext(WebSocket webSocket, Dictionary<string, string> queryParams)
        {
            _socket = webSocket;
            QueryParams = queryParams;
        }

        public Task Send(byte[] data) =>
            _socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

        public Task Close(WebSocketCloseStatus closeStatus, string statusDescription) =>
            _socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
    }
}