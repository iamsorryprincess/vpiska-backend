using System.Threading;
using System.Threading.Tasks;
using MediatR;
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

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<EventResponse>), 200)]
        public Task<DomainResponse<EventResponse>> Create([FromBody] CreateEventRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public Task<DomainResponse> Close([FromBody] CloseEventRequest request, CancellationToken cancellationToken) =>
            _mediator.Send(request, cancellationToken);
    }
}