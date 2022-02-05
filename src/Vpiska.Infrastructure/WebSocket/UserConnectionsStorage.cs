using System;
using System.Collections.Concurrent;
using System.Linq;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class UserConnectionsStorage : IUserConnectionsStorage
    {
        private readonly ConcurrentDictionary<Guid, Range> _connections;

        public UserConnectionsStorage()
        {
            _connections = new ConcurrentDictionary<Guid, Range>();
        }

        public bool AddConnection(Guid connectionId) => _connections.TryAdd(connectionId, new Range());

        public bool RemoveConnection(Guid connectionId) => _connections.TryRemove(connectionId, out _);

        public bool SetRange(Guid connectionId, double x, double y, double horizontalRange, double verticalRange)
        {
            if (!_connections.TryGetValue(connectionId, out var range))
            {
                return false;
            }
            
            var halfHorizontalRange = horizontalRange / 2;
            var halfVerticalRange = verticalRange / 2;
            range.XLeft = x - halfHorizontalRange;
            range.XRight = x + halfHorizontalRange;
            range.YBottom = y - halfVerticalRange;
            range.YTop = y + halfVerticalRange;
            return true;
        }

        public Guid[] GetConnectionsByRange(double x, double y)
        {
            return _connections
                .Where(pair => pair.Value.XLeft <= x && pair.Value.XRight >= x &&
                               pair.Value.YBottom <= y && pair.Value.YTop >= y)
                .Select(pair => pair.Key)
                .ToArray();
        }
        
        private sealed class Range
        {
            public double XLeft;
            public double XRight;
            public double YBottom;
            public double YTop;
        }
    }
}