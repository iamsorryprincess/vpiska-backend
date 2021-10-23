using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICheckAreaRepository
    {
        Task<bool> IsExist(string areaName);
    }
}