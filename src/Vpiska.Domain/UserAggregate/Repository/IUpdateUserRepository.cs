using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface IUpdateUserRepository
    {
        Task<bool> IsPhoneExist(string phone);

        Task<bool> IsNameExist(string name);

        Task<bool> Update(Guid id, string name, string phone, string imageId);
    }
}