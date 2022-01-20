using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Domain.User.Interfaces
{
    public interface INotificationService
    {
        Task PushNotification(string token, Dictionary<string, string> notificationBody,
            CancellationToken cancellationToken = default);
    }
}