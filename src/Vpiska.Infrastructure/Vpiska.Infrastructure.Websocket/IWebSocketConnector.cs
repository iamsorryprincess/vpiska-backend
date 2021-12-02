using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    public interface IWebSocketConnector
    {
        Task OnConnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams);

        Task OnDisconnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams);
    }
}