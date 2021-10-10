using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Serilog;
using Vpiska.Domain;

namespace Vpiska.Firebase
{
    public sealed class NotificationService : INotificationService
    {
        private const string FirebaseToken = "test";
        
        private readonly ILogger _logger;

        public NotificationService(ILogger logger)
        {
            _logger = logger;
        }
        
        public async Task SendVerificationCode(int code)
        {
            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "code", code.ToString() }
                },
                Token = FirebaseToken,
                Notification = new Notification()
                {
                    Title = "Код подтверждения",
                    Body = "Введите код для входа"
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.Information(response);
        }
    }
}