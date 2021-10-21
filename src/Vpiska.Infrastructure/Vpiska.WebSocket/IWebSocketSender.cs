using System;
using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketSender<in TMessage>
    {
        Task SendMessage(Guid connectionId, TMessage message);
    }
}