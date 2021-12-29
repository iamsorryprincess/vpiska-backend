using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vpiska.WebSocket;

namespace Vpiska.Chat.Receivers
{
    public sealed class ChatReceiver : IWebSocketReceiver
    {
        public Task Receive(Guid connectionId, byte[] data, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            throw new NotImplementedException();
        }
    }
}