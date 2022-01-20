using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Requests;
using Vpiska.Api.Responses;
using Vpiska.Domain.Event.Commands.AddMediaCommand;
using Vpiska.Domain.Event.Commands.CloseEventCommand;
using Vpiska.Domain.Event.Commands.CreateEventCommand;
using Vpiska.Domain.Event.Commands.RemoveMediaCommand;
using Vpiska.Domain.Event.Queries.GetByIdQuery;
using Vpiska.Domain.Event.Queries.GetEventsQuery;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), 200)]
        public async Task<IActionResult> Create(
            [FromServices] ICommandHandler<CreateEventCommand, EventResponse> commandHandler,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCommand(GetUserId());
            var result = await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse<EventResponse>.Success(result));
        }

        [HttpPost("range")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventShortResponse[]>), 200)]
        public async Task<IActionResult> GetByRange(
            [FromServices] IQueryHandler<GetEventsQuery, List<EventShortResponse>> queryHandler,
            [FromBody] GetEventsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await queryHandler.HandleAsync(query, cancellationToken);
            return Ok(ApiResponse<List<EventShortResponse>>.Success(result));
        }

        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventResponse>), 200)]
        public async Task<IActionResult> GetById(
            [FromServices] IQueryHandler<GetByIdQuery, EventResponse> queryHandler,
            [FromBody] GetByIdQuery query,
            CancellationToken cancellationToken)
        {
            var result = await queryHandler.HandleAsync(query, cancellationToken);
            return Ok(ApiResponse<EventResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Close(
            [FromServices] ICommandHandler<CloseEventCommand> commandHandler,
            [FromBody] IdRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCloseEventCommand(GetUserId());
            await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> AddMedia(
            [FromServices] ICommandHandler<AddMediaCommand> commandHandler,
            [FromForm] AddMediaRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCommand(GetUserId());
            await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> RemoveMedia(
            [FromServices] ICommandHandler<RemoveMediaCommand> commandHandler,
            [FromBody] RemoveMediaRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCommand(GetUserId());
            await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        private string GetUserId()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParse(userId.Value, out _))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return userId.Value;
        }
    }
}