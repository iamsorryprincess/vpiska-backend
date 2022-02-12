using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.EventClosedEvent
{
    internal sealed class EventClosedHandler : IEventHandler<EventClosedEvent>
    {
        private readonly ILogger<EventClosedHandler> _logger;
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventSender _eventSender;
        private readonly IUserConnectionsStorage _userConnectionsStorage;
        private readonly IUserSender _userSender;
        private readonly IEventState _eventState;

        public EventClosedHandler(ILogger<EventClosedHandler> logger,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender,
            IEventState eventState)
        {
            _logger = logger;
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventSender = eventSender;
            _userConnectionsStorage = userConnectionsStorage;
            _userSender = userSender;
            _eventState = eventState;
        }

        public async Task Handle(EventClosedEvent domainEvent)
        {
            if (_eventConnectionsStorage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _eventConnectionsStorage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await Task.WhenAll(connections.Select(_eventSender.SendCloseStatus));
                }

                if (!_eventConnectionsStorage.RemoveEventGroup(domainEvent.EventId))
                {
                    _logger.LogWarning("Can't remove event group {}", domainEvent.EventId);
                }
            }
            
            var rangeConnections = _userConnectionsStorage.GetConnectionsByRange(domainEvent.Coordinates.X, domainEvent.Coordinates.Y);

            if (rangeConnections.Any())
            {
                await _userSender.SendEventClosed(rangeConnections, domainEvent.EventId);
            }
            
            await _eventState.RemoveData(domainEvent.EventId);
        }
    }
}