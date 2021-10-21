using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.UserAggregate.Repository
{
    public interface IGetByIdRepository
    {
        Task<User> GetById(Guid id);
    }
}