using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Domain.User.Commands.ChangePasswordCommand
{
    internal sealed class ChangePasswordHandler : ValidationCommandHandler<ChangePasswordCommand>
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        
        public ChangePasswordHandler(IValidator<ChangePasswordCommand> validator,
            IUserRepository repository,
            IPasswordHasher passwordHasher) : base(validator)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        protected override async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var isNotFound = !await _repository.UpdateByFieldAsync("_id", command.Id, "password",
                _passwordHasher.HashPassword(command.Password), cancellationToken);

            if (isNotFound)
            {
                throw new UserNotFoundException(Constants.UserNotFound);
            }
        }
    }
}