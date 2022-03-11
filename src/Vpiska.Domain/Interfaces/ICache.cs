using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface ICache<T> where T : class, new()
    {
        Task<T> Get(string key);

        Task<bool> Set(string key, T data);

        Task<bool> Remove(string key);
    }
}