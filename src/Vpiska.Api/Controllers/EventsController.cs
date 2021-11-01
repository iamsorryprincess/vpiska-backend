using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost("all")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<EventInfoResponse[]>), 200)]
        public Task<DomainResponse<EventInfoResponse[]>> GetAll([FromBody] GetEventsRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);
        
        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<EventResponse>), 200)]
        public Task<DomainResponse<EventResponse>> GetById([FromBody] GetEventRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<EventResponse>), 200)]
        public Task<DomainResponse<EventResponse>> Create([FromBody] Models.CreateEventRequest body,
            CancellationToken cancellationToken)
        {
            var request = new CreateEventRequest()
            {
                OwnerId = GetUserId(),
                Name = body.Name,
                Coordinates = body.Coordinates,
                Address = body.Address,
                Area = body.Area
            };
            
            return _mediator.Send(request, cancellationToken);
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public Task<DomainResponse> Close([FromBody] Models.CloseEventRequest body,
            CancellationToken cancellationToken)
        {
            var request = new CloseEventRequest()
            {
                EventId = body.EventId.Value,
                OwnerId = GetUserId()
            };
            
            return _mediator.Send(request, cancellationToken);
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DomainResponse<MediaResponse>), 200)]
        public async Task<DomainResponse<MediaResponse>> AddMedia([FromForm] Models.AddMediaRequest body,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await using var stream = new MemoryStream();
            await body.Media.CopyToAsync(stream, cancellationToken);

            var request = new AddMediaRequest()
            {
                EventId = body.EventId.Value,
                OwnerId = userId,
                MediaData = stream.ToArray(),
                ContentType = body.Media.ContentType
            };

            var response = await _mediator.Send(request, cancellationToken);
            return response;
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public Task<DomainResponse> RemoveMedia([FromBody] Models.RemoveMediaRequest body,
            CancellationToken cancellationToken)
        {
            var request = new RemoveMediaRequest()
            {
                EventId = body.EventId.Value,
                OwnerId = GetUserId(),
                MediaLink = body.MediaLink
            };

            return _mediator.Send(request, cancellationToken);
        }

        private Guid GetUserId()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParse(userId.Value, out var result))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return result;
        }
    }
}