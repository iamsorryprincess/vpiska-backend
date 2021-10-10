using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FSharp.Core;
using Vpiska.Domain;
using Vpiska.Domain.Commands;
using Vpiska.Domain.Errors;
using Vpiska.Domain.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        private readonly CommandHandler _commandHandler;

        public UserController(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
        
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Response<LoginResponse>), 200)]
        public Task<Response<LoginResponse>> Create([FromBody] CreateUserArgs request)
        {
            var command = Command.NewCreateUser(request);
            return _commandHandler.Handle<LoginResponse>(command);
        }
        
        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Response<LoginResponse>), 200)]
        public Task<Response<LoginResponse>> Login([FromBody] LoginUserArgs request)
        {
            var command = Command.NewLoginUser(request);
            return _commandHandler.Handle<LoginResponse>(command);
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Response), 200)]
        public Task<Response> SetVerificationCode([FromBody] CodeArgs request)
        {
            var command = Command.NewSetCode(request);
            return _commandHandler.HandleWithNoResponse(command);
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Response<LoginResponse>), 200)]
        public Task<Response<LoginResponse>> CheckVerificationCode([FromBody] CheckCodeArgs request)
        {
            var command = Command.NewCheckCode(request);
            return _commandHandler.Handle<LoginResponse>(command);
        }

        [Authorize]
        [HttpPost("password")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Response), 200)]
        public Task<Response> ChangePassword([FromBody] ChangePasswordArgs request)
        {
            var command = Command.NewChangePassword(request);
            return _commandHandler.HandleWithNoResponse(command);
        }

        [Authorize]
        [HttpPatch]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response), 200)]
        public async Task<Response> UpdateUser([FromForm] Models.UpdateUserArgs request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return Domain.Responses.Response.error(new[] { AppError.create(ValidationError.IdIsEmpty) });
            }
            
            if (request.Image != null)
            {
                await using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream);
                var commandArgs = new UpdateUserArgs(request.Id, request.Name, request.Phone,
                    FSharpValueOption<Stream>.Some(stream), request.Image.ContentType);
                var command = Command.NewUpdateUser(commandArgs);
                var result = await _commandHandler.HandleWithNoResponse(command);
                return result;
            }

            var commandArgs2 = new UpdateUserArgs(request.Id, request.Name, request.Phone,
                FSharpValueOption<Stream>.None, null);
            var command2 = Command.NewUpdateUser(commandArgs2);
            var result2 = await _commandHandler.HandleWithNoResponse(command2);
            return result2;
        }
    }
}