using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Api.Models.Event;
using Vpiska.Api.Orleans;
using Vpiska.WebSocket;

namespace Vpiska.Api.Receivers
{
    public sealed class ChatReceiver : IWebSocketReceiver
    {
        private readonly IClusterClient _clusterClient;

        public ChatReceiver(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task Receive(Guid connectionId, byte[] data, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var message = new ChatMessage()
            {
                UserId = identityParams["Id"],
                UserName = identityParams["Name"],
                UserImageId = identityParams["ImageId"],
                Message = Encoding.UTF8.GetString(data)
            };

            var grain = _clusterClient.GetGrain<IEventGrain>(queryParams["eventId"]);
            return grain.AddChatMessage(message);
        }
    }
}