using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class CloseEventHandler : RequestHandlerBase<CloseEventRequest>
    {
        private readonly ICheckEventRepository _checkEventRepository;
        private readonly ICloseEventRepository _closeRepository;

        public CloseEventHandler(ICheckEventRepository checkEventRepository,
            ICloseEventRepository closeRepository)
        {
            _checkEventRepository = checkEventRepository;
            _closeRepository = closeRepository;
        }
        
        public override async Task<DomainResponse> Handle(CloseEventRequest request, CancellationToken cancellationToken)
        {
            var isEventNotExist = !await _checkEventRepository.IsEventExist(request.EventId);

            if (isEventNotExist)
            {
                return Error(DomainErrorConstants.EventNotFound);
            }
            
            var isNotOwner = !await _checkEventRepository.CheckOwnership(request.EventId, request.OwnerId);

            if (isNotOwner)
            {
                return Error(DomainErrorConstants.UserNotOwner);
            }

            await _closeRepository.RemoveEvent(request.EventId);
            return Success();
        }
    }
}