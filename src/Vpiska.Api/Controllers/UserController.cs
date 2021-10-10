using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Requests;
using Vpiska.Domain.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public Task<DomainResponse<LoginResponse>> RegisterUser(
            [FromServices] ICommandHandler<CreateUserRequest, LoginResponse> commandHandler,
            [FromBody] CreateUserRequest request) => commandHandler.Handle(request);

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public Task<DomainResponse<LoginResponse>> Login(
            [FromServices] ICommandHandler<LoginRequest, LoginResponse> commandHandler,
            [FromBody] LoginRequest request) => commandHandler.Handle(request);

        [HttpPost("code")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(DomainResponse), 500)]
        public Task<DomainResponse> SetVerificationCode(
            [FromServices] ICommandHandler<CodeRequest> commandHandler,
            [FromBody] CodeRequest request) => commandHandler.Handle(request);

        /*[HttpGet("code")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> CheckVerificationCode([FromQuery] CheckCodeRequest request)
        {
            var user = await _storage.GetUserByPhone(request.Phone);

            if (user == null)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "phone", "Пользователь с таким номером не найден" } });
            }

            if (user.VerificationCode != request.Code)
            {
                return BadRequest(new Dictionary<string, string>()
                    { { "code", "Неверный код" } });
            }
            
            var result = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = JwtOptions.GetEncodedJwt(user.Id, user.Name, user.ImageUrl)
            };

            return Ok(result);
        }

        [Authorize]
        [HttpPost("password")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var isSuccess = await _storage.ChangePassword(request.Id, request.Password.HashPassword());

            if (!isSuccess)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "id", "Пользователь с таким id не найден" } });
            }

            return NoContent();
        }

        [Authorize]
        [HttpPatch]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> UpdateUser([FromServices] IFileStorage fileStorage, [FromForm] UpdateUserRequest request)
        {
            var user = await _storage.GetById(request.Id);

            if (user == null)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "id", "Пользователь с таким id не найден" } });
            }

            string imageUrl = null;
            
            if (request.Image != null)
            {
                await using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream);
                
                if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                {
                    imageUrl = await fileStorage.UploadFile(user.ImageUrl, request.Image.ContentType, stream);
                }
                else
                {
                    imageUrl = await fileStorage.UploadFile(Guid.NewGuid().ToString("N"), request.Image.ContentType, stream);
                }
            }

            await _storage.Update(request.Id, request.Name, request.Phone, imageUrl);
            return NoContent();
        }*/
    }
}