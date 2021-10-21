using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class CheckCodeHandler : RequestHandlerBase<CheckCodeRequest, DomainResponse<LoginResponse>>
    {
        private readonly IGetByPhoneRepository _repository;
        private readonly IJwtService _jwt;

        public CheckCodeHandler(IGetByPhoneRepository repository, IJwtService jwt)
        {
            _repository = repository;
            _jwt = jwt;
        }
        
        public override async Task<DomainResponse<LoginResponse>> Handle(CheckCodeRequest request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByPhone(request.Phone);

            if (user == null)
            {
                return Error<LoginResponse>(DomainErrorConstants.UserByPhoneNotFound);
            }

            if (user.VerificationCode != request.Code)
            {
                return Error<LoginResponse>(DomainErrorConstants.InvalidCode);
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