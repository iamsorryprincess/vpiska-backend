using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketInteracting<TListener> where TListener : IWebSocketListener
    {
        Task SendMessage<TMessage>(Guid connectionId, string route, TMessage message) where TMessage : class, new();

        Task SendRawMessage(Guid connectionId, string route, string message);

        Task Close(Guid connectionId);
    }
}