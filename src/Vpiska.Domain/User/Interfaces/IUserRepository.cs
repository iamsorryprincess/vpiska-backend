using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Models;

namespace Vpiska.Domain.User.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<CheckResult> CheckPhoneAndName(string phone, string name, CancellationToken cancellationToken = default);

        Task<CheckResult> CheckPhoneAndNameWithEmptyParams(string phone, string name,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateUser(string id, string name, string phone, string imageId,
            CancellationToken cancellationToken = default);
    }
}