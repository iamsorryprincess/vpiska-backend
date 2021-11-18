using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Application;
using Vpiska.Application.Event;
using Vpiska.Domain.Event;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly EventMobileHttpHandler _handler;

        public EventsController(EventMobileHttpHandler handler)
        {
            _handler = handler;
        }

        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public Task<ObjectResult> Create([FromBody] Models.CreateEventArgs body)
        {
            var args = new CreateEventArgs(GetUserId(), body.Name, body.Coordinates, body.Address, body.Area);
            var command = Command.NewCreateEvent(args);
            return _handler.Handle(command);
        }

        private string GetUserId()
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

            return userId.Value;
        }
    }
}