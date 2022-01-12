using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketReceiver
    {
        Task Receive(Guid connectionId, byte[] data, Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams);
    }
}