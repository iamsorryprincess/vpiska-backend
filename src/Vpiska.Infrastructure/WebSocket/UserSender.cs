using System;
using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class UserSender : IUserSender
    {
        private readonly IWebSocketInteracting<RangeListener> _webSocketInteracting;

        public UserSender(IWebSocketInteracting<RangeListener> webSocketInteracting)
        {
            _webSocketInteracting = webSocketInteracting;
        }

        public Task SendEventUpdatedInfo(Guid[] connections, EventUpdatedInfo eventUpdatedInfo) => Task.WhenAll(
            connections.Select(connectionId =>
                _webSocketInteracting.SendMessage(connectionId, "eventUpdated", eventUpdatedInfo)));

        public Task SendEventCreated(Guid[] connections, EventCreatedInfo eventCreatedInfo) => Task.WhenAll(
            connections.Select(connectionId =>
                _webSocketInteracting.SendMessage(connectionId, "eventCreated", eventCreatedInfo)));

        public Task SendEventClosed(Guid[] connections, string eventId) => Task.WhenAll(
            connections.Select(connectionId =>
                _webSocketInteracting.SendRawMessage(connectionId, "eventClosed", eventId)));
    }
}