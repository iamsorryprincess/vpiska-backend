using System.Threading.Tasks;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IGetEventsRepository
    {
        Task<EventInfoResponse[]> GetEvents(string area);
    }
}