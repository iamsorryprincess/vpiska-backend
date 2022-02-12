using System;

namespace Vpiska.WebSocket
{
    public interface IWebSocketExceptionHandler
    {
        void Handle(WebSocketContext socketContext, Exception exception);
    }
}