using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Domain.Constants;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Models;
using Vpiska.Domain.Requests;
using Vpiska.Domain.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private static readonly Random Random = new Random();

        private readonly IUserRepository _repository;

        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }
        
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public async Task<DomainResponse<LoginResponse>> Create(
            [FromServices] IPasswordSecurityService passwordSecurity,
            [FromServices] IJwtService jwt,
            [FromBody] CreateUserRequest request)
        {
            var existingUsers = await _repository.GetByNameAndPhone(request.Name, request.Phone);

            if (existingUsers.Count > 0)
            {
                var (isPhoneExist, isNameExist) = existingUsers.Aggregate((false, false), (tuple, user) =>
                {
                    if (!tuple.Item1)
                    {
                        tuple.Item1 = user.Phone == request.Phone;
                    }

                    if (!tuple.Item2)
                    {
                        tuple.Item2 = user.Name == request.Name;
                    }

                    return tuple;
                });

                switch (isPhoneExist)
                {
                    case true when !isNameExist:
                        return Error<LoginResponse>(DomainErrorConstants.PhoneAlreadyUse);
                    case false when isNameExist:
                        return Error<LoginResponse>(DomainErrorConstants.NameAlreadyUse);
                    case true:
                        return Error<LoginResponse>(DomainErrorConstants.PhoneAlreadyUse, DomainErrorConstants.NameAlreadyUse);
                }
            }

            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Phone = request.Phone,
                PhoneCode = UserConstants.PhoneCode,
                Password = passwordSecurity.HashPassword(request.Password)
            };

            await _repository.Create(user);

            var response = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Success(response);
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public async Task<DomainResponse<LoginResponse>> Login(
            [FromServices] IPasswordSecurityService passwordSecurity,
            [FromServices] IJwtService jwt,
            [FromBody] LoginUserRequest request)
        {
            var user = await _repository.GetByPhone(request.Phone);

            if (user == null)
            {
                return Error<LoginResponse>(DomainErrorConstants.UserByPhoneNotFound);
            }

            if (!passwordSecurity.CheckPassword(user.Password, request.Password))
            {
                return Error<LoginResponse>(DomainErrorConstants.InvalidPassword);
            }

            var response = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Success(response);
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public async Task<DomainResponse> SetCode(
            [FromServices] IFirebaseCloudMessaging cloudMessaging,
            [FromBody] SetCodeRequest request)
        {
            var code = Random.Next(111111, 777777);
            var isSuccess = await _repository.SetCode(request.Phone, code);

            if (!isSuccess)
            {
                return Error(DomainErrorConstants.UserByPhoneNotFound);
            }

            await cloudMessaging.SendVerificationCode(code);
            return Success();
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public async Task<DomainResponse<LoginResponse>> CheckCode(
            [FromServices] IJwtService jwt,
            [FromBody] CheckCodeRequest request)
        {
            var user = await _repository.GetByPhone(request.Phone);

            if (user == null)
            {
                return Error<LoginResponse>(DomainErrorConstants.UserByPhoneNotFound);
            }

            if (user.VerificationCode != request.Code)
            {
                return Error<LoginResponse>(DomainErrorConstants.InvalidCode);
            }

            var response = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Success(response);
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public async Task<DomainResponse> ChangePassword(
            [FromServices] IPasswordSecurityService passwordSecurity,
            [FromBody] ChangePasswordRequest request)
        {
            var newPassword = passwordSecurity.HashPassword(request.Password);
            var isSuccess = await _repository.ChangePassword(request.Id, newPassword);
            return !isSuccess ? Error(DomainErrorConstants.UserNotFound) : Success();
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public async Task<DomainResponse> UpdateUser(
            [FromServices] IFirebaseStorage storage,
            [FromForm] UpdateUserRequest request,
            IFormFile image)
        {
            var user = await _repository.GetById(request.Id);

            if (user == null)
            {
                return Error(DomainErrorConstants.UserNotFound);
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var isPhoneExist = await _repository.IsPhoneExist(request.Phone);

                if (isPhoneExist)
                {
                    return Error(DomainErrorConstants.PhoneAlreadyUse);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var isNameExist = await _repository.IsNameExist(request.Phone);
                
                if (isNameExist)
                {
                    return Error(DomainErrorConstants.NameAlreadyUse);
                }
            }

            if (image != null)
            {
                await using var stream = new MemoryStream();
                await image.CopyToAsync(stream);
                var imageId = string.IsNullOrWhiteSpace(user.ImageId)
                    ? await storage.UploadFile(Guid.NewGuid().ToString(), image.ContentType, stream)
                    : await storage.UploadFile(user.ImageId, image.ContentType, stream);
                await _repository.Update(request.Id, request.Name, request.Phone, imageId);
                return Success();
            }

            await _repository.Update(request.Id, request.Name, request.Phone, user.ImageId);
            return Success();
        }

        private static DomainResponse Error(params string[] errorCodes) =>
            DomainResponse.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());

        private static DomainResponse<TResponse> Error<TResponse>(params string[] errorCodes)
            where TResponse : class =>
            DomainResponse<TResponse>.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());
        
        private static DomainResponse Success() => DomainResponse.CreateSuccess();

        private static DomainResponse<TResponse> Success<TResponse>(TResponse response) where TResponse : class =>
            DomainResponse<TResponse>.CreateSuccess(response);
    }
}