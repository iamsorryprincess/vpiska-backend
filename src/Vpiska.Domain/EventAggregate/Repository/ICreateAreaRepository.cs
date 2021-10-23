using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICreateAreaRepository
    {
        Task<bool> Create(string areaName);
    }
}