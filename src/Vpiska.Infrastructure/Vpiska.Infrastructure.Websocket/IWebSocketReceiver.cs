using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    public interface IWebSocketReceiver
    {
        Task Receive(Guid connectionId, byte[] data, Dictionary<string, string> queryParams);
    }
}