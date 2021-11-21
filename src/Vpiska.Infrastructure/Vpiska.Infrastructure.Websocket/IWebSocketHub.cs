using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    internal interface IWebSocketHub
    {
        Task<Guid> AddConnection(WebSocketUserContext userContext, WebSocket webSocket,
            Dictionary<string, string> queryParams);

        Task<bool> TryCloseConnection(Guid connectionId);

        Task ReceiveMessage(Guid connectionId, byte[] data);
    }
}