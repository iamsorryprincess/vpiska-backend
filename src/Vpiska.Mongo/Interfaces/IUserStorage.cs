using System.Threading.Tasks;
using Vpiska.Mongo.Models;

namespace Vpiska.Mongo.Interfaces
{
    public interface IUserStorage
    {
        Task<User> GetById(string id);
        
        Task<string> Create(User user);

        Task<bool> Update(string id, string name, string phone, string imageUrl);

        Task<NamePhoneCheckResult> CheckInfo(string name, string phone);

        Task<User> GetUserByPhone(string phone);

        Task<bool> SetVerificationCode(string phone, int code);

        Task<bool> ChangePassword(string id, string password);
    }
}