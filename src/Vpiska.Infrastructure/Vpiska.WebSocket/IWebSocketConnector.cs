using System;
using System.Collections.Generic;

namespace Vpiska.WebSocket
{
    public interface IWebSocketConnector<TMessage>
    {
        void OnConnect(Guid connectionId, WebSocketUserContext userContext, Dictionary<string, string> queryParams);

        void OnDisconnect(Guid connectionId, Dictionary<string, string> queryParams);
    }
}