using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICreateEventRepository
    {
        Task<bool> IsOwnerHasEvent(string areaName, Guid ownerId);

        Task<bool> Create(string area, Event @event);
    }
}