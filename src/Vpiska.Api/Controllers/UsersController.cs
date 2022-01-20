using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Requests;
using Vpiska.Api.Responses;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Commands.ChangePasswordCommand;
using Vpiska.Domain.User.Commands.CreateUserCommand;
using Vpiska.Domain.User.Commands.LoginUserCommand;
using Vpiska.Domain.User.Commands.SetCodeCommand;
using Vpiska.Domain.User.Commands.UpdateUserCommand;
using Vpiska.Domain.User.Queries.CheckCodeQuery;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Create(
            [FromServices] ICommandHandler<CreateUserCommand, LoginResponse> commandHandler,
            [FromBody] CreateUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.Success(result));
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Login(
            [FromServices] ICommandHandler<LoginUserCommand, LoginResponse> commandHandler,
            [FromBody] LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.Success(result));
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> SetCode(
            [FromServices] ICommandHandler<SetCodeCommand> commandHandler,
            [FromBody] SetCodeCommand command,
            CancellationToken cancellationToken)
        {
            await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> CheckCode(
            [FromServices] IQueryHandler<CheckCodeQuery, LoginResponse> queryHandler,
            [FromBody] CheckCodeQuery query,
            CancellationToken cancellationToken)
        {
            var result = await queryHandler.HandleAsync(query, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> ChangePassword(
            [FromServices] ICommandHandler<ChangePasswordCommand> commandHandler,
            [FromBody] ChangePasswordCommand command,
            CancellationToken cancellationToken)
        {
            await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImageIdResponse>), 200)]
        public async Task<IActionResult> Update(
            [FromServices] ICommandHandler<UpdateUserCommand, ImageIdResponse> commandHandler,
            [FromForm] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCommand();
            var result = await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse<ImageIdResponse>.Success(result));
        }
    }
}