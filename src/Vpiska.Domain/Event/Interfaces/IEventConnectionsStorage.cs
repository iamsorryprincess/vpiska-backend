using System;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventConnectionsStorage
    {
        bool IsEventGroupExist(string eventId);

        bool CreateEventGroup(string eventId);

        bool RemoveEventGroup(string eventId);

        bool AddConnection(string eventId, Guid connectionId, string userId);

        bool RemoveConnection(string eventId, Guid connectionId);

        Guid[] GetConnections(string eventId);
    }
}