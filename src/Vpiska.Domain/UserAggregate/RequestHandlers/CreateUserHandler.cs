using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class CreateUserHandler : RequestHandlerBase<CreateUserRequest, DomainResponse<LoginResponse>>
    {
        private readonly ICreateUserRepository _repository;
        private readonly IPasswordSecurityService _passwordSecurity;
        private readonly IJwtService _jwt;

        public CreateUserHandler(ICreateUserRepository repository,
            IPasswordSecurityService passwordSecurity,
            IJwtService jwt)
        {
            _repository = repository;
            _passwordSecurity = passwordSecurity;
            _jwt = jwt;
        }
        
        public override async Task<DomainResponse<LoginResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var existingUsers = await _repository.GetByNameAndPhone(request.Name, request.Phone);

            if (existingUsers.Count > 0)
            {
                var (isPhoneExist, isNameExist) = existingUsers.Aggregate((false, false), (tuple, user) =>
                {
                    if (!tuple.Item1)
                    {
                        tuple.Item1 = user.Phone == request.Phone;
                    }

                    if (!tuple.Item2)
                    {
                        tuple.Item2 = user.Name == request.Name;
                    }

                    return tuple;
                });

                switch (isPhoneExist)
                {
                    case true when !isNameExist:
                        return Error<LoginResponse>(DomainErrorConstants.PhoneAlreadyUse);
                    case false when isNameExist:
                        return Error<LoginResponse>(DomainErrorConstants.NameAlreadyUse);
                    case true:
                        return Error<LoginResponse>(DomainErrorConstants.PhoneAlreadyUse, DomainErrorConstants.NameAlreadyUse);
                }
            }

            var user = new User(Guid.NewGuid(), request.Name, UserConstants.PhoneCode, request.Phone, null,
                _passwordSecurity.HashPassword(request.Password), 0);
            await _repository.Create(user);

            var response = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = _jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Success(response);
        }
    }
}