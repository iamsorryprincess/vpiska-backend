using System;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IUserConnectionsStorage
    {
        bool AddConnection(Guid connectionId);

        bool RemoveConnection(Guid connectionId);

        bool SetRange(Guid connectionId, double x, double y, double horizontalRange, double verticalRange);

        Guid[] GetConnectionsByRange(double x, double y);
    }
}