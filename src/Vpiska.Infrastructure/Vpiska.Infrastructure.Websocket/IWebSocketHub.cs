using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    internal interface IWebSocketHub
    {
        Task<Guid> AddConnection(WebSocket webSocket,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams);

        Task<bool> TryCloseConnection(Guid connectionId,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams);

        Task ReceiveMessage(Guid connectionId, byte[] data,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams);
    }
}