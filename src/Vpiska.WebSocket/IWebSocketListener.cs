using System.Threading.Tasks;

namespace Vpiska.WebSocket
{
    public interface IWebSocketListener
    {
        Task OnConnect(WebSocketContext socketContext);

        Task OnDisconnect(WebSocketContext socketContext);

        Task Receive(WebSocketContext socketContext, string route, string message);
    }
}