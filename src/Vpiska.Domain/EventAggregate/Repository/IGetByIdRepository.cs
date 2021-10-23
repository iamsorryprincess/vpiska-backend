using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IGetByIdRepository
    {
        Task<Event> GetById(Guid eventId);
    }
}