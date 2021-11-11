using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vpiska.ApiChat.Connectors;
using Vpiska.WebSocket;

namespace Vpiska.ApiChat.Receivers
{
    public sealed class ChatReceiver : IWebSocketReceiver
    {
        private readonly IWebSocketSender<ChatConnector> _sender;

        public ChatReceiver(IWebSocketSender<ChatConnector> sender)
        {
            _sender = sender;
        }

        public Task Receive(Guid connectionId, byte[] data, Dictionary<string, string> queryParams)
        {
            Console.WriteLine("receive");
            return _sender.SendMessage(connectionId, Encoding.UTF8.GetBytes("test"));
        }
    }
}