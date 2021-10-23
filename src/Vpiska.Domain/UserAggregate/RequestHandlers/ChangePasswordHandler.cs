using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class ChangePasswordHandler : RequestHandlerBase<ChangePasswordRequest>
    {
        private readonly IChangePasswordRepository _repository;
        private readonly IPasswordSecurityService _passwordSecurity;
        
        public ChangePasswordHandler(IChangePasswordRepository repository,
            IPasswordSecurityService passwordSecurity)
        {
            _repository = repository;
            _passwordSecurity = passwordSecurity;
        }
        
        public override async Task<DomainResponse> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var newPassword = _passwordSecurity.HashPassword(request.Password);
            var isSuccess = await _repository.ChangePassword(request.Id, newPassword);
            return isSuccess ? Success() : Error(DomainErrorConstants.UserNotFound);
        }
    }
}