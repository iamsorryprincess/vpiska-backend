using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain.User.Commands.LoginUserCommand
{
    internal sealed class LoginUserHandler : ICommandHandler<LoginUserCommand, LoginResponse>
    {
        private readonly IValidator<LoginUserCommand> _validator;
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IIdentityService _identityService;
        
        public LoginUserHandler(IValidator<LoginUserCommand> validator,
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            IIdentityService identityService)
        {
            _validator = validator;
            _repository = repository;
            _passwordHasher = passwordHasher;
            _identityService = identityService;
        }

        public async Task<LoginResponse> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var user = await _repository.GetByFieldAsync("phone", command.Phone, cancellationToken);

            if (user == null)
            {
                throw new UserNotFoundException(Constants.UserByPhoneNotFound);
            }

            if (!_passwordHasher.VerifyHashPassword(user.Password, command.Password))
            {
                throw new InvalidPasswordException();
            }

            return new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = _identityService.GetAccessToken(user)
            };
        }
    }
}