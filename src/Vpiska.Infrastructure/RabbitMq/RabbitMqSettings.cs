using System;

namespace Vpiska.Infrastructure.RabbitMq
{
    internal sealed class RabbitMqSettings
    {
        public string Host { get; }
        
        public string Username { get; }

        public string Password { get; }
        
        public Action<RabbitMqHostedService> SetupAction { get; }

        public RabbitMqSettings(string host, string username, string password, Action<RabbitMqHostedService> setupAction)
        {
            Host = host;
            Username = username;
            Password = password;
            SetupAction = setupAction;
        }
    }
}