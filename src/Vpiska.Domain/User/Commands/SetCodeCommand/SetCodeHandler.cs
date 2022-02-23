using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Domain.User.Commands.SetCodeCommand
{
    internal sealed class SetCodeHandler : ICommandHandler<SetCodeCommand>
    {
        private static readonly Random Random = new();

        private readonly IValidator<SetCodeCommand> _validator;
        private readonly IUserRepository _repository;
        private readonly INotificationService _notificationService;

        public SetCodeHandler(IValidator<SetCodeCommand> validator,
            IUserRepository repository,
            INotificationService notificationService)
        {
            _validator = validator;
            _repository = repository;
            _notificationService = notificationService;
        }


        public async Task HandleAsync(SetCodeCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var code = Random.Next(111111, 777777);
            var isNotSuccess = !await _repository.UpdateByFieldAsync("phone", command.Phone, "verificationCode", code,
                cancellationToken);

            if (isNotSuccess)
            {
                throw new UserNotFoundException(Constants.UserByPhoneNotFound);
            }

            await _notificationService.PushNotification(command.Token, new Dictionary<string, string>()
            {
                { "code", code.ToString() },
                { "body", "Введите код для входа" },
                { "title", "Код подтверждения" }
            }, cancellationToken);
        }
    }
}