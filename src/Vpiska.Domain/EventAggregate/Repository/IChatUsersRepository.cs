using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IChatUsersRepository
    {
        Task<bool> AddUser(Guid id, string name, string imageId);

        Task<bool> RemoveUser(Guid id);
    }
}