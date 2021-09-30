using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vpiska.Api.Auth;
using Vpiska.Api.Dto;
using Vpiska.Mongo.Interfaces;
using Vpiska.Mongo.Models;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        private static readonly Random Random = new Random();

        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUserStorage _storage;

        public UserController(ILogger<UserController> logger, IMapper mapper, IUserStorage storage)
        {
            _logger = logger;
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
        public async Task<IActionResult> SetVerificationCode([FromBody] CodeRequest request)
        {
            var code = Random.Next(111111, 777777);
            var isSuccess = await _storage.SetVerificationCode(request.Phone, code);

            if (!isSuccess)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "phone", "Пользователь с таким номером не найден" } });
            }
            
            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "myData", code.ToString() }
                },
                Token = request.FirebaseToken,
                Notification = new Notification()
                {
                    Title = "Код подтверждения",
                    Body = "Введите код для входа"
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation(response);
            return Ok();
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

            return Ok();
        }

        [Authorize]
        [HttpPatch]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 400)]
        [ProducesResponseType(typeof(Dictionary<string, string>), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> UpdateUser([FromQuery] UpdateUserRequest request)
        {
            string imageUrl = null;
            if (request.Image != null)
            {
                using (var stream = new MemoryStream())
                {
                    await request.Image.CopyToAsync(stream);
                    var bytes = stream.ToArray();
                }
            }

            var isSuccess = await _storage.Update(request.Id, request.Name, request.Phone, imageUrl);

            if (!isSuccess)
            {
                return NotFound(new Dictionary<string, string>()
                    { { "id", "Пользователь с таким id не найден" } });
            }
            
            return Ok();
        }
    }
}