using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain.User.Commands.CreateUserCommand
{
    internal sealed class CreateUserHandler : ICommandHandler<CreateUserCommand, LoginResponse>
    {
        private readonly IValidator<CreateUserCommand> _validator;
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IIdentityService _identityService;
        
        public CreateUserHandler(IValidator<CreateUserCommand> validator,
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            IIdentityService identityService)
        {
            _validator = validator;
            _repository = repository;
            _passwordHasher = passwordHasher;
            _identityService = identityService;
        }

        public async Task<LoginResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var checkResult = await _repository.CheckPhoneAndName(command.Phone, command.Name, cancellationToken);
            switch (checkResult.IsNameExist)
            {
                case true when !checkResult.IsPhoneExist:
                    throw new UserNameAlreadyExistException();
                case false when checkResult.IsPhoneExist:
                    throw new UserPhoneAlreadyExistException();
                case true:
                    throw new UserPhoneAndNameAlreadyExistException();
                default:
                {
                    var user = new User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = command.Name,
                        Phone = command.Phone,
                        PhoneCode = "+7",
                        Password = _passwordHasher.HashPassword(command.Password)
                    };

                    await _repository.InsertAsync(user, cancellationToken);
                    return new LoginResponse(_identityService.GetAccessToken(user), null, user);
                }
            }
        }
    }
}