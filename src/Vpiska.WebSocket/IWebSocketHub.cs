using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketHub
    {
        Task<Guid> AddConnection(System.Net.WebSockets.WebSocket webSocket,
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