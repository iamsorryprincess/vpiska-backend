using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Vpiska.Orleans.Interfaces
{
    internal interface IAreaGrain : IGrainWithStringKey
    {
        Task<bool> AddEvent(IEventGrain eventGrain);

        Task<bool> RemoveEvent(IEventGrain eventGrain);

        Task<IReadOnlyList<IEventGrain>> GetEventGrains();

        Task<bool> CheckOwnerId(Guid ownerId);
    }
}