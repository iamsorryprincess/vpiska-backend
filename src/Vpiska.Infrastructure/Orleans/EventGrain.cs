using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Infrastructure.Orleans
{
    [StorageProvider(ProviderName = Constants.StorageProvider)]
    internal sealed class EventGrain : Grain<Event>, IEventGrain
    {
        private readonly IEventBus _eventBus;
        private readonly EventHandlersResolver _eventHandlersResolver;
        
        private IDisposable _eventBusSubscription;
        private StreamSubscriptionHandle<IDomainEvent> _streamSubscription;

        public EventGrain(IEventBus eventBus, EventHandlersResolver eventHandlersResolver)
        {
            _eventBus = eventBus;
            _eventHandlersResolver = eventHandlersResolver;
        }
        
        public Task<Event> GetData() => Task.FromResult(State.Id == null ? null : State);

        public Task Init(Event data)
        {
            State = data;
            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeAsync(string streamNamespace)
        {
            if (State.Id == null)
            {
                return false;
            }

            var sp = GetStreamProvider(Constants.StreamMessageProvider);
            var stream = sp.GetStream<IDomainEvent>(Guid.Parse(State.Id), streamNamespace);

            if (_eventBusSubscription != null || _streamSubscription != null)
            {
                return false;
            }

            _eventBusSubscription = _eventBus.EventStream.Subscribe(domainEvent => stream.OnNextAsync(domainEvent));
            _streamSubscription = await stream.SubscribeAsync((domainEvent, _) => _eventHandlersResolver.Resolve(domainEvent));
            return true;
        }

        public async Task<bool> UnsubscribeAsync(string streamNamespace)
        {
            if (State.Id == null)
            {
                return false;
            }
            
            if (_eventBusSubscription != null)
            {
                _eventBusSubscription.Dispose();
                _eventBusSubscription = null;
            }

            if (_streamSubscription != null)
            {
                await _streamSubscription.UnsubscribeAsync();
                _streamSubscription = null;
            }
            
            return true;
        }

        public async Task<bool> Close()
        {
            if (State.Id == null)
            {
                return false;
            }
            
            State = new Event();
            
            if (_eventBusSubscription != null)
            {
                _eventBusSubscription.Dispose();
                _eventBusSubscription = null;
            }

            if (_streamSubscription != null)
            {
                await _streamSubscription.UnsubscribeAsync();
                _streamSubscription = null;
            }

            await ClearStateAsync();
            return true;
        }

        public Task AddChatMessage(ChatMessage chatMessage)
        {
            if (State.Id == null)
            {
                return Task.CompletedTask;
            }

            State.ChatData.Add(chatMessage);
            return Task.CompletedTask;
        }

        public Task<ChatMessage[]> GetChatMessages() =>
            Task.FromResult(State.Id == null
                ? Array.Empty<ChatMessage>()
                : State.ChatData.ToArray());

        public Task<bool> AddUserInfo(UserInfo userInfo)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            if (State.Users.Any(x => x.UserId == userInfo.UserId))
            {
                return Task.FromResult(false);
            }

            State.Users.Add(userInfo);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserInfo(string userId)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            var userInfo = State.Users.FirstOrDefault(x => x.UserId == userId);
            return Task.FromResult(userInfo != null && State.Users.Remove(userInfo));
        }

        public Task<bool> AddMediaLink(string mediaLink)
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

        public Task<bool> RemoveMediaLink(string mediaLink) =>
            Task.FromResult(State.Id != null && State.MediaLinks.Remove(mediaLink));
    }
}