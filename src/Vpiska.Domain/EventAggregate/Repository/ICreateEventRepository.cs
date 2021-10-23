using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICreateEventRepository
    {
        Task<bool> IsOwnerHasEvent(Guid ownerId);

        Task<Guid> Create(string area, Event @event);
    }
}