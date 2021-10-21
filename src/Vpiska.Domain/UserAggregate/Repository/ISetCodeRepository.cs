using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface ISetCodeRepository
    {
        Task<bool> SetCode(string phone, int code);
    }
}