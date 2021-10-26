using System;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class CreateEventHandler : RequestHandlerBase<CreateEventRequest, EventResponse>
    {
        private readonly ICheckAreaRepository _areaRepository;
        private readonly ICreateEventRepository _eventRepository;

        public CreateEventHandler(ICreateEventRepository repository,
            ICheckAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
            _eventRepository = repository;
        }
        
        public override async Task<DomainResponse<EventResponse>> Handle(CreateEventRequest request, CancellationToken cancellationToken)
        {
            var isAreaNotExist = !await _areaRepository.IsExist(request.Area);

            if (isAreaNotExist)
            {
                return Error(DomainErrorConstants.AreaNotFound);
            }
            
            var isOwnerEventExist = await _eventRepository.IsOwnerHasEvent(request.Area, request.OwnerId);

            if (isOwnerEventExist)
            {
                return Error(DomainErrorConstants.OwnerAlreadyHasEvent);
            }

            var @event = new Event(Guid.NewGuid(), request.OwnerId, request.Name, request.Coordinates, request.Address);
            var isFail = !await _eventRepository.Create(request.Area, @event);

            if (isFail)
            {
                return Error(DomainErrorConstants.AreaAlreadyHasEvent);
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