using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketInteracting<TConnector> where TConnector : IWebSocketConnector
    {
        Task SendMessage(Guid connectionId, byte[] data);

        Task Close(Guid connectionId);
    }
}