using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Domain.User.Commands.ChangePasswordCommand
{
    internal sealed class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand>
    {
        private readonly IValidator<ChangePasswordCommand> _validator;
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        
        public ChangePasswordHandler(IValidator<ChangePasswordCommand> validator,
            IUserRepository repository,
            IPasswordHasher passwordHasher)
        {
            _validator = validator;
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            
            var isNotFound = !await _repository.UpdateByFieldAsync("_id", command.Id, "password",
                _passwordHasher.HashPassword(command.Password), cancellationToken);

            if (isNotFound)
            {
                throw new UserNotFoundException(Constants.UserNotFound);
            }
        }
    }
}