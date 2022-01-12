using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using Vpiska.Api.Common;
using Vpiska.Api.Connectors;
using Vpiska.Api.Models.Event;
using Vpiska.WebSocket;

namespace Vpiska.Api.Orleans
{
    [StorageProvider(ProviderName = "PubSubStore")]
    public sealed class EventGrain : Grain<Event>, IEventGrain
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private readonly Storage _storage;
        private readonly IWebSocketInteracting<ChatConnector> _webSocket;

        private IAsyncStream<ChatMessage> _stream;
        private StreamSubscriptionHandle<ChatMessage> _subscription;

        public EventGrain(Storage storage, IWebSocketInteracting<ChatConnector> webSocket)
        {
            _storage = storage;
            _webSocket = webSocket;
        }

        public Task<Event> GetData() => State.Id == null
            ? Task.FromResult<Event>(null)
            : Task.FromResult(State);

        public async Task Init(Event data)
        {
            State = data;
            var eventId = data.Id;
            var sp = GetStreamProvider("SMSProvider");
            _stream = sp.GetStream<ChatMessage>(Guid.Parse(eventId), "chat");
            _subscription = await _stream.SubscribeAsync(
                (message, _) =>
                {
                    var json = JsonSerializer.Serialize(message, JsonSerializerOptions);
                    var body = Encoding.UTF8.GetBytes(json);
                    return Task.WhenAll(_storage.GetConnections(eventId)
                        .Select(connectionId => _webSocket.SendMessage(connectionId, body)));
                });
        }

        public async Task Close()
        {
            await _subscription.UnsubscribeAsync();
            var eventId = this.GetPrimaryKeyString();
            var connections = _storage.GetConnections(eventId);
            await Task.WhenAll(connections.Select(connectionId => _webSocket.Close(connectionId)));
            _storage.RemoveEventGroup(eventId);
            await ClearStateAsync();
        }

        public Task<bool> AddMedia(string mediaLink)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            if (State.MediaLinks.Contains(mediaLink))
            {
                return Task.FromResult(false);
            }
            
            State.MediaLinks.Add(mediaLink);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveMedia(string mediaLink) =>
            Task.FromResult(State.Id != null && State.MediaLinks.Remove(mediaLink));

        public async Task<bool> AddUser(string userId)
        {
            if (State.Id == null)
            {
                return false;
            }

            if (State.Users.Any(x => x.Id == userId))
            {
                return false;
            }

            State.Users.Add(new UserInfo() { Id = userId });

            if (State.ChatData.Count > 0)
            {
                var connectionId = _storage.GetUserConnectionId(userId);

                if (connectionId != Guid.Empty)
                {
                    foreach (var chatMessage in State.ChatData)
                    {
                        var json = JsonSerializer.Serialize(chatMessage, JsonSerializerOptions);
                        var data = Encoding.UTF8.GetBytes(json);
                        await _webSocket.SendMessage(connectionId, data);
                    }
                }
            }

            return true;
        }

        public Task<bool> RemoveUser(string userId)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            var user = State.Users.FirstOrDefault(x => x.Id == userId);
            return Task.FromResult(user != null && State.Users.Remove(user));
        }

        public async Task AddChatMessage(ChatMessage chatMessage)
        {
            if (State.Id == null)
            {
                return;
            }
            
            State.ChatData.Add(chatMessage);
            await _stream.OnNextAsync(chatMessage);
        }
    }
}