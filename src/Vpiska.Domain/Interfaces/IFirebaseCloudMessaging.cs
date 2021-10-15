using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IFirebaseCloudMessaging
    {
        Task SendVerificationCode(int code);
    }
}