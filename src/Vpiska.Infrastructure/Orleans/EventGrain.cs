using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Events.EventCreatedEvent;
using Vpiska.Domain.Event.Events.EventUpdatedEvent;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Events.UserConnectedEvent;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;

namespace Vpiska.Infrastructure.Orleans
{
    [StorageProvider(ProviderName = Constants.StorageProvider)]
    internal sealed class EventGrain : Grain<Event>, IEventGrain
    {
        private readonly IEventBus _eventBus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<IDisposable> _busSubscriptions;

        private Guid _eventCreatedStreamId;
        private StreamSubscriptionHandle<EventCreatedEvent> _eventCreatedSubscription;

        private Guid _chatStreamId;
        private StreamSubscriptionHandle<ChatMessageEvent> _chatSubscription;

        private Guid _eventUpdatedStreamId;
        private StreamSubscriptionHandle<EventUpdatedEvent> _eventUpdatedSubscription;

        private Guid _eventClosedStreamId;
        private StreamSubscriptionHandle<EventClosedEvent> _eventClosedSubscription;

        private Guid _mediaAddedStreamId;
        private StreamSubscriptionHandle<MediaAddedEvent> _mediaAddedSubscription;

        private Guid _mediaRemovedStreamId;
        private StreamSubscriptionHandle<MediaRemovedEvent> _mediaRemovedSubscription;

        private Guid _userConnectedStreamId;
        private StreamSubscriptionHandle<UserConnectedEvent> _userConnectedSubscription;

        private Guid _userDisconnectedStreamId;
        private StreamSubscriptionHandle<UserDisconnectedEvent> _userDisconnectedSubscription;

        public EventGrain(IEventBus eventBus, IServiceScopeFactory scopeFactory)
        {
            _eventBus = eventBus;
            _scopeFactory = scopeFactory;
            _busSubscriptions = new List<IDisposable>();
        }

        public Task<Event> GetData() => Task.FromResult(State.Id == null ? null : State);

        public async Task Init(Event data)
        {
            State = data;
            var streamProvider = GetStreamProvider(Constants.StreamMessageProvider);
            (_eventCreatedStreamId, _eventCreatedSubscription) = await SubscribeAsync<EventCreatedEvent>(streamProvider);
            (_chatStreamId, _chatSubscription) = await SubscribeAsync<ChatMessageEvent>(streamProvider);
            (_eventUpdatedStreamId, _eventUpdatedSubscription) = await SubscribeAsync<EventUpdatedEvent>(streamProvider);
            (_eventClosedStreamId, _eventClosedSubscription) = await SubscribeAsync<EventClosedEvent>(streamProvider);
            (_mediaAddedStreamId, _mediaAddedSubscription) = await SubscribeAsync<MediaAddedEvent>(streamProvider);
            (_mediaRemovedStreamId, _mediaRemovedSubscription) = await SubscribeAsync<MediaRemovedEvent>(streamProvider);
            (_userConnectedStreamId, _userConnectedSubscription) = await SubscribeAsync<UserConnectedEvent>(streamProvider);
            (_userDisconnectedStreamId, _userDisconnectedSubscription) = await SubscribeAsync<UserDisconnectedEvent>(streamProvider);
        }

        public async Task<bool> Close()
        {
            if (State.Id == null)
            {
                return false;
            }
            
            var streamProvider = GetStreamProvider(Constants.StreamMessageProvider);

            await _eventCreatedSubscription.UnsubscribeAsync();
            var eventCreatedStream = streamProvider.GetStream<EventCreatedEvent>(_eventCreatedStreamId, Constants.EventStreamNamespace);
            await eventCreatedStream.OnCompletedAsync();
            _eventCreatedSubscription = null;
            
            await _chatSubscription.UnsubscribeAsync();
            var chatStream = streamProvider.GetStream<ChatMessageEvent>(_chatStreamId, Constants.EventStreamNamespace);
            await chatStream.OnCompletedAsync();
            _chatSubscription = null;

            await _eventUpdatedSubscription.UnsubscribeAsync();
            var eventUpdatedStream = streamProvider.GetStream<EventUpdatedEvent>(_eventUpdatedStreamId, Constants.EventStreamNamespace);
            await eventUpdatedStream.OnCompletedAsync();
            _eventUpdatedSubscription = null;

            await _eventClosedSubscription.UnsubscribeAsync();
            var eventClosedStream = streamProvider.GetStream<EventClosedEvent>(_eventClosedStreamId, Constants.EventStreamNamespace);
            await eventClosedStream.OnCompletedAsync();
            _eventClosedSubscription = null;

            await _mediaAddedSubscription.UnsubscribeAsync();
            var mediaAddedStream = streamProvider.GetStream<MediaAddedEvent>(_mediaAddedStreamId, Constants.EventStreamNamespace);
            await mediaAddedStream.OnCompletedAsync();
            _mediaAddedSubscription = null;

            await _mediaRemovedSubscription.UnsubscribeAsync();
            var mediaRemovedStream = streamProvider.GetStream<MediaRemovedEvent>(_mediaRemovedStreamId, Constants.EventStreamNamespace);
            await mediaRemovedStream.OnCompletedAsync();
            _mediaRemovedSubscription = null;

            await _userConnectedSubscription.UnsubscribeAsync();
            var userConnectedStream = streamProvider.GetStream<UserConnectedEvent>(_userConnectedStreamId, Constants.EventStreamNamespace);
            await userConnectedStream.OnCompletedAsync();
            _userConnectedSubscription = null;

            await _userDisconnectedSubscription.UnsubscribeAsync();
            var userDisconnectedStream = streamProvider.GetStream<UserDisconnectedEvent>(_userDisconnectedStreamId, Constants.EventStreamNamespace);
            await userDisconnectedStream.OnCompletedAsync();
            _userDisconnectedSubscription = null;

            foreach (var busSubscription in _busSubscriptions)
            {
                busSubscription?.Dispose();
            }
            
            _busSubscriptions.Clear();
            State = new Event();
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

        public Task<bool> UpdateData(string address, Coordinates coordinates)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            State.Address = address;
            State.Coordinates = coordinates;
            return Task.FromResult(true);
        }

        private async Task<(Guid, StreamSubscriptionHandle<TEvent>)> SubscribeAsync<TEvent>(
            IStreamProvider streamProvider) where TEvent : class, IDomainEvent
        {
            var streamId = Guid.NewGuid();
            var stream = streamProvider.GetStream<TEvent>(streamId, Constants.EventStreamNamespace);

            var streamSubscription = await stream.SubscribeAsync(async (domainEvent, _) =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                await eventHandler.Handle(domainEvent);
            });

            var busSubscription = _eventBus.EventStream.Subscribe(data =>
            {
                if (data is TEvent domainEvent)
                {
                    stream.OnNextAsync(domainEvent);
                }
            });
            
            _busSubscriptions.Add(busSubscription);
            return (streamId, streamSubscription);
        }
    }
}