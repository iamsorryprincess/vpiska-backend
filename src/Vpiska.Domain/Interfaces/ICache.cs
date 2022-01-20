using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface ICache<TModel>
    {
        Task<TModel> GetData(string id);

        Task SetData(TModel data);

        Task<bool> RemoveData(string id);
    }
}