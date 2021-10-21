using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface IChangePasswordRepository
    {
        Task<bool> ChangePassword(Guid id, string password);
    }
}