using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vpiska.Domain.Models;

namespace Vpiska.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> Create(User user);

        Task<User> GetById(Guid id);

        Task<User> GetByPhone(string phone);

        Task<List<User>> GetByNameAndPhone(string name, string phone);

        Task<bool> SetCode(string phone, int code);

        Task<bool> IsPhoneExist(string phone);

        Task<bool> IsNameExist(string name);

        Task<bool> ChangePassword(Guid id, string password);

        Task<bool> Update(Guid id, string name, string phone, string imageId);
    }
}