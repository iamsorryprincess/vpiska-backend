using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Application;
using Vpiska.Application.Event;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands;

namespace Vpiska.Api.Rest.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly QueryHandler _queryHandler;
        private readonly EventCommandHandler _commandHandler;

        public EventsController(QueryHandler queryHandler, EventCommandHandler commandHandler)
        {
            _queryHandler = queryHandler;
            _commandHandler = commandHandler;
        }

        [HttpPost("all")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ShortEventResponse[]>), 200)]
        public Task<ObjectResult> GetAll([FromBody] GetEventsArgs args)
        {
            var query = Query.NewGetEvents(args);
            return Handle(query);
        }

        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public Task<ObjectResult> GetById([FromBody] GetEventArgs args)
        {
            var query = Query.NewGetEvent(args);
            return Handle(query);
        }

        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public Task<ObjectResult> Create([FromBody] Application.Event.CreateEventArgs args)
        {
            var command = args.toCommand(Guid.NewGuid().ToString("N"), GetUserId());
            return Handle(command);
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> Close([FromBody] Application.Event.CloseEventArgs args)
        {
            var command = args.toCommand(GetUserId());
            return Handle(command);
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<MediaArgs>), 200)]
        public async Task<ObjectResult> AddMedia([FromForm] Application.Event.AddMediaArgs args)
        {
            var command = await args.toCommand(GetUserId(), Guid.NewGuid().ToString("N"));
            var result = await Handle(command);
            return result;
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> RemoveMedia([FromBody] Application.Event.RemoveMediaArgs args)
        {
            var command = args.toCommand(GetUserId());
            return Handle(command);
        }

        private string GetUserId()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParseExact(userId.Value, "N", out _))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return userId.Value;
        }

        private async Task<ObjectResult> Handle(EventCommand command)
        {
            var result = await _commandHandler.Handle(command);
            return Http.mapToMobileEventResult(result);
        }

        private async Task<ObjectResult> Handle(Query query)
        {
            var result = await _queryHandler.Handle(query);
            return Http.mapToMobileResponseResult(result);
        }
    }
}