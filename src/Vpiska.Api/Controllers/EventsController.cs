using System;
using System.IO;
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
        private readonly CommandHandler _commandHandler;

        public EventsController(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
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
            return Handle(command);
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> Close([FromBody] Models.CloseEventArgs body)
        {
            var args = new CloseEventArgs(GetUserId(), body.EventId);
            var command = Command.NewCloseEvent(args);
            return Handle(command);
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AddMediaResponseArgs>), 200)]
        public async Task<ObjectResult> AddMedia([FromForm] Models.AddMediaArgs body)
        {
            await using var stream = new MemoryStream();
            await body.Media.CopyToAsync(stream);
            var args = new AddMediaArgs(body.EventId, GetUserId(), stream.ToArray(), body.Media.ContentType);
            var command = Command.NewAddMedia(args);
            var result = await Handle(command);
            return result;
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> RemoveMedia([FromBody] Models.RemoveMediaArgs body)
        {
            var args = new RemoveMediaArgs(body.EventId, GetUserId(), body.MediaLink);
            var command = Command.NewRemoveMedia(args);
            return Handle(command);
        }

        private string GetUserId()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParse(userId.Value, out var _))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return userId.Value;
        }

        private async Task<ObjectResult> Handle(Command command)
        {
            var result = await _commandHandler.Handle(command);
            return Http.mapToMobileResult(result);
        }
    }
}