using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Auth;
using Vpiska.Api.Dto;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Models;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        private static readonly Random Random = new Random();

        private readonly IMapper _mapper;
        private readonly IUserStorage _storage;

        public UserController(IMapper mapper, IUserStorage storage)
        {
            _mapper = mapper;
            _storage = storage;
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(LoginResponse), 201)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserRequest request)
        {
            var checkResult = await _storage.CheckInfo(request.Name, request.Phone);

            if (checkResult != null)
            {
                var dictResult = new Dictionary<string, string>();
                
                if (checkResult.IsPhoneExist)
                {
                    dictResult.Add("phone", "Номер телефона занят");
                }
                
                if (checkResult.IsNameExist)
                {
                    dictResult.Add("name", "Имя пользователя занято");
                }

                return BadRequest(dictResult);
            }
            
            var user = _mapper.Map<User>(request);
            await _storage.Create(user);
            var result = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = JwtOptions.GetEncodedJwt(user.Id, user.Name, user.ImageUrl),
            };
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _storage.GetUserByPhone(request.Phone);

            if (user == null)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "phone", "Пользователь с таким номером не найден" } });
            }

            if (!user.Password.CheckHashPassword(request.Password))
            {
                return BadRequest(new Dictionary<string, string>()
                    { { "password", "Неверный пароль" } });
            }

            var result = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = JwtOptions.GetEncodedJwt(user.Id, user.Name, user.ImageUrl)
            };

            return Ok(result);
        }
        
        [HttpPost("code")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> SetVerificationCode([FromServices] INotificationService notificationService, [FromBody] CodeRequest request)
        {
            var code = Random.Next(111111, 777777);
            var isSuccess = await _storage.SetVerificationCode(request.Phone, code);

            if (!isSuccess)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "phone", "Пользователь с таким номером не найден" } });
            }

            await notificationService.SendVerificationCode(code, request.FirebaseToken);
            return NoContent();
        }
        
        [HttpGet("code")]
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
        }
    }
}