using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketReceiver<in TMessage>
    {
        Task Receive(Guid connectionId, TMessage message, Dictionary<string, string> queryParams);
    }
}