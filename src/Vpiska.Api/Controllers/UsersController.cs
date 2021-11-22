using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Application;
using Vpiska.Application.User;
using Vpiska.Domain.User;
using UpdateUserArgs = Vpiska.Application.User.UpdateUserArgs;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly CommandHandler _commandHandler;

        public UsersController(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> Create([FromBody] CreateUserArgs args)
        {
            var command = Command.NewCreate(args);
            return Handle(command);
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> Login([FromBody] LoginUserArgs args)
        {
            var command = Command.NewLogin(args);
            return Handle(command);
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> SetCode([FromBody] CodeArgs args)
        {
            var command = Command.NewSetCode(args);
            return Handle(command);
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> CheckCode([FromBody] CheckCodeArgs args)
        {
            var command = Command.NewCheckCode(args);
            return Handle(command);
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> ChangePassword([FromBody] ChangePasswordArgs args)
        {
            var command = Command.NewChangePassword(args);
            return Handle(command);
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<ObjectResult> UpdateUser([FromForm] UpdateUserArgs args)
        {
            var command = await args.toCommand();
            var result = await Handle(command);
            return result;
        }

        private async Task<ObjectResult> Handle(Command command)
        {
            var result = await _commandHandler.Handle(command);
            return Http.mapToMobileResult(result);
        }
    }
}