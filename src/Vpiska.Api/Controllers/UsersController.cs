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
        private readonly UserMobileHttpHandler _handler;

        public UsersController(UserMobileHttpHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> Create([FromBody] CreateUserArgs args)
        {
            var command = Command.NewCreate(args);
            return _handler.Handle(command);
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> Login([FromBody] LoginUserArgs args)
        {
            var command = Command.NewLogin(args);
            return _handler.Handle(command);
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> SetCode([FromBody] CodeArgs args)
        {
            var command = Command.NewSetCode(args);
            return _handler.Handle(command);
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public Task<ObjectResult> CheckCode([FromBody] CheckCodeArgs args)
        {
            var command = Command.NewCheckCode(args);
            return _handler.Handle(command);
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> ChangePassword([FromBody] ChangePasswordArgs args)
        {
            var command = Command.NewChangePassword(args);
            return _handler.Handle(command);
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public Task<ObjectResult> UpdateUser([FromForm] UpdateUserArgs args) => _handler.HandleUpdateUser(args);
    }
}