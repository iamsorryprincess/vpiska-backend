using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketSender<TConnector> where TConnector : IWebSocketConnector
    {
        Task SendMessage(Guid connectionId, byte[] data);
    }
}