using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    internal interface IWebSocketHub
    {
        Guid AddConnection(WebSocketUserContext userContext, System.Net.WebSockets.WebSocket webSocket,
            Dictionary<string, string> queryParams);

        Task<bool> TryCloseConnection(Guid connectionId);

        Task ReceiveMessage(Guid connectionId, byte[] data);
    }
}