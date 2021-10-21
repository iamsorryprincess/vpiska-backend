using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface IGetByPhoneRepository
    {
        Task<User> GetByPhone(string phone);
    }
}