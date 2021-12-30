using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo
{
    public interface IUserRepository : IMongoRepository<User>
    {
        Task<(bool isPhoneExist, bool isNameExist)> CheckPhoneAndName(string phone, string name,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateUser(string id, string name, string phone, string imageId,
            CancellationToken cancellationToken = default);
    }
}