using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Serilog;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Firebase
{
    internal sealed class FirebaseCloudMessaging : IFirebaseCloudMessaging
    {
        private readonly ILogger _logger;

        public FirebaseCloudMessaging(ILogger logger)
        {
            _logger = logger;
        }
        
        public async Task SendVerificationCode(int code, string token)
        {
            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "code", code.ToString() },
                    { "body", "Введите код для входа" },
                    { "title", "Код подтверждения" }
                },
                Token = token
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}