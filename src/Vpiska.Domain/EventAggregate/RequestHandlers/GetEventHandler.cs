using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class GetEventHandler : RequestHandlerBase<GetEventRequest, EventResponse>
    {
        private readonly IGetByIdRepository _idRepository;

        public GetEventHandler(IGetByIdRepository idRepository)
        {
            _idRepository = idRepository;
        }
        
        public override async Task<DomainResponse<EventResponse>> Handle(GetEventRequest request, CancellationToken cancellationToken)
        {
            var @event = await _idRepository.GetById(request.Id);

            if (@event == null)
            {
                return Error(DomainErrorConstants.EventNotFound);
            }

            var response = new EventResponse()
            {
                Id = @event.Id,
                Name = @event.Name,
                Coordinates = @event.Coordinates,
                Address = @event.Address,
                Users = @event.Users,
                MediaLinks = @event.MediaLinks
            };

            return Success(response);
        }
    }
}