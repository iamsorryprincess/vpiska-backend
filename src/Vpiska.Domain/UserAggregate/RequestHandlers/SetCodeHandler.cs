using System;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class SetCodeHandler : RequestHandlerBase<SetCodeRequest, DomainResponse>
    {
        private static readonly Random Random = new Random();
        
        private readonly ISetCodeRepository _repository;
        private readonly IFirebaseCloudMessaging _cloudMessaging;

        public SetCodeHandler(ISetCodeRepository repository, IFirebaseCloudMessaging cloudMessaging)
        {
            _repository = repository;
            _cloudMessaging = cloudMessaging;
        }
        
        public override async Task<DomainResponse> Handle(SetCodeRequest request, CancellationToken cancellationToken)
        {
            var code = Random.Next(111111, 777777);
            var isSuccess = await _repository.SetCode(request.Phone, code);

            if (!isSuccess)
            {
                return Error(DomainErrorConstants.UserByPhoneNotFound);
            }

            await _cloudMessaging.SendVerificationCode(code, request.FirebaseToken);
            return Success();
        }
    }
}