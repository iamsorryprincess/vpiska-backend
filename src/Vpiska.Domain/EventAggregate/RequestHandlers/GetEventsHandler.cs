using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class GetEventsHandler : RequestHandlerBase<GetEventsRequest, EventInfoResponse[]>
    {
        private readonly ICheckAreaRepository _areaRepository;
        private readonly IGetEventsRepository _eventsRepository;

        public GetEventsHandler(ICheckAreaRepository areaRepository,
            IGetEventsRepository eventsRepository)
        {
            _areaRepository = areaRepository;
            _eventsRepository = eventsRepository;
        }
        
        public override async Task<DomainResponse<EventInfoResponse[]>> Handle(GetEventsRequest request, CancellationToken cancellationToken)
        {
            var isAreaNotFound = !await _areaRepository.IsExist(request.Area);

            if (isAreaNotFound)
            {
                return Error(DomainErrorConstants.AreaNotFound);
            }

            var response = await _eventsRepository.GetEvents(request.Area);
            return Success(response);
        }
    }
}