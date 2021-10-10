using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface INotificationService
    {
        Task SendVerificationCode(int code);
    }
}