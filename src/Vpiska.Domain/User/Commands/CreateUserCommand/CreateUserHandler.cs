using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain.User.Commands.CreateUserCommand
{
    internal sealed class CreateUserHandler : ValidationCommandHandler<CreateUserCommand, LoginResponse>
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IIdentityService _identityService;
        
        public CreateUserHandler(IValidator<CreateUserCommand> validator,
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            IIdentityService identityService) : base(validator)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _identityService = identityService;
        }

        protected override async Task<LoginResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
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
                    var user = new User(Guid.NewGuid().ToString(),
                        command.Name, "+7",
                        command.Phone, null,
                        _passwordHasher.HashPassword(command.Password), 0);
                    
                    await _repository.InsertAsync(user, cancellationToken);

                    var response = new LoginResponse()
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        ImageId = user.ImageId,
                        AccessToken = _identityService.GetAccessToken(user)
                    };

                    return response;
                }
            }
        }
    }
}