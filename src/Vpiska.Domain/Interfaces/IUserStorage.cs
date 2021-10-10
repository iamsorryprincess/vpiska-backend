using System.Threading.Tasks;
using Vpiska.Domain.Models;

namespace Vpiska.Domain.Interfaces
{
    public interface IUserStorage
    {
        Task<UserModel> GetById(string id);

        Task<string> Create(UserModel user);

        Task<bool> Update(string id, string name, string phone, string imageUrl);

        Task<NamePhoneCheckModel> CheckInfo(string name, string phone);

        Task<UserModel> GetUserByPhone(string phone);

        Task<bool> SetVerificationCode(string phone, int code);

        Task<bool> ChangePassword(string id, string password);
    }
}