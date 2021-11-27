using System;
using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Websocket
{
    public interface IWebSocketInteracting<TConnector> where TConnector : IWebSocketConnector
    {
        Task SendMessage(Guid connectionId, byte[] data);

        Task Close(Guid connectionId);
    }
}