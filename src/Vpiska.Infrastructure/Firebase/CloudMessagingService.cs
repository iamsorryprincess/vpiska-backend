using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Infrastructure.Firebase
{
    internal sealed class CloudMessagingService : INotificationService
    {
        public Task PushNotification(string token, Dictionary<string, string> notificationBody,
            CancellationToken cancellationToken) => FirebaseMessaging.DefaultInstance.SendAsync(new Message()
        {
            Data = notificationBody,
            Token = token
        }, cancellationToken);
    }
}