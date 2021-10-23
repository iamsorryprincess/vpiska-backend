using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class LoginUserHandler : RequestHandlerBase<LoginUserRequest, LoginResponse>
    {
        private readonly IGetByPhoneRepository _repository;
        private readonly IPasswordSecurityService _passwordSecurity;
        private readonly IJwtService _jwt;
        
        public LoginUserHandler(IGetByPhoneRepository repository,
            IPasswordSecurityService passwordSecurity,
            IJwtService jwt)
        {
            _repository = repository;
            _passwordSecurity = passwordSecurity;
            _jwt = jwt;
        }
        
        public override async Task<DomainResponse<LoginResponse>> Handle(LoginUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByPhone(request.Phone);

            if (user == null)
            {
                return Error(DomainErrorConstants.UserByPhoneNotFound);
            }

            if (_passwordSecurity.IsPasswordInvalid(user.Password, request.Password))
            {
                return Error(DomainErrorConstants.InvalidPassword);
            }

            var response = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = _jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Success(response);
        }
    }
}