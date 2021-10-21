using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface ICreateUserRepository
    {
        Task<List<User>> GetByNameAndPhone(string name, string phone);
        
        Task<Guid> Create(User user);
    }
}