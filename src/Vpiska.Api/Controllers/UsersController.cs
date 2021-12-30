using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using FluentValidation;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vpiska.Api.Constants;
using Vpiska.Api.Extensions;
using Vpiska.Api.Requests.User;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.User;
using Vpiska.Api.Settings;
using Vpiska.Domain.Models;
using Vpiska.Mongo;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private static readonly Random Random = new();

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Create([FromServices] IValidator<CreateUserRequest> validator,
            [FromServices] IUserRepository repository,
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var (isPhoneExist, isNameExist) =
                await repository.CheckPhoneAndName(request.Phone, request.Name, cancellationToken);

            switch (isNameExist)
            {
                case true when !isPhoneExist:
                    return Ok(ApiResponse.Error(UserConstants.NameAlreadyUse));
                case false when isPhoneExist:
                    return Ok(ApiResponse.Error(UserConstants.PhoneAlreadyUse));
                case true:
                    return Ok(ApiResponse.Error(UserConstants.NameAlreadyUse, UserConstants.PhoneAlreadyUse));
                default:
                {
                    var user = new User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Phone = request.Phone,
                        PhoneCode = UserConstants.PhoneCode,
                        Name = request.Name,
                        Password = request.Password.HashPassword()
                    };

                    await repository.InsertAsync(user, cancellationToken);
                    
                    var response = new LoginResponse()
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        ImageId = user.ImageId,
                        AccessToken = Jwt.Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
                    };
                    
                    return Ok(ApiResponse<LoginResponse>.Success(response));
                }
            }
        }

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Login([FromServices] IValidator<LoginUserRequest> validator,
            [FromServices] IUserRepository repository,
            [FromBody] LoginUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var user = await repository.GetAsync(x => x.Phone, request.Phone, cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserByPhoneNotFound));
            }

            if (!user.Password.CheckPassword(request.Password))
            {
                return Ok(ApiResponse.Error(UserConstants.InvalidPassword));
            }
            
            var response = new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = Jwt.Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };
            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> SetVerificationCode([FromServices] IValidator<SetCodeRequest> validator,
            [FromServices] IUserRepository repository,
            [FromBody] SetCodeRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var code = Random.Next(111111, 777777);
            var isNotSuccess = !await repository.UpdateAsync(x => x.Phone, request.Phone,
                x => x.VerificationCode, code,
                cancellationToken);

            if (isNotSuccess)
            {
                return Ok(ApiResponse.Error(UserConstants.UserByPhoneNotFound));
            }

            await FirebaseMessaging.DefaultInstance.SendAsync(new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "code", code.ToString() },
                    { "body", "Введите код для входа" },
                    { "title", "Код подтверждения" }
                },
                Token = request.Token
            }, cancellationToken);

            return Ok(ApiResponse.Success());
        }

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> CheckCode([FromServices] IValidator<CheckCodeRequest> validator,
            [FromServices] IUserRepository repository,
            [FromBody] CheckCodeRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var user = await repository.GetAsync(x => x.Phone, request.Phone, cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserByPhoneNotFound));
            }

            if (user.VerificationCode != request.Code)
            {
                return Ok(ApiResponse.Error(UserConstants.InvalidCode));
            }
            
            var response = new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = Jwt.Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };
            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> ChangePassword([FromServices] IValidator<ChangePasswordRequest> validator,
            [FromServices] IUserRepository repository,
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var isSuccess = await repository.UpdateAsync(x => x.Id, request.Id, x => x.Password,
                request.Password.HashPassword(), cancellationToken);
            return Ok(isSuccess ? ApiResponse.Success() : ApiResponse.Error(UserConstants.UserNotFound));
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImageIdResponse>), 200)]
        public async Task<IActionResult> Update([FromServices] IValidator<UpdateUserRequest> validator,
            [FromServices] IUserRepository repository,
            [FromServices] StorageClient fileStorage,
            [FromServices] IOptions<FirebaseSettings> firebaseSettings,
            [FromForm] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Phone) &&
                request.Image == null)
            {
                return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = null }));
            }

            var user = await repository.GetAsync(x => x.Id, request.Id, cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse<ImageIdResponse>.Error(UserConstants.UserNotFound));
            }

            var (isPhoneExist, isNameExist) =
                await repository.CheckPhoneAndName(request.Phone, request.Name, cancellationToken);

            switch (isNameExist)
            {
                case true when !isPhoneExist:
                    return Ok(ApiResponse<ImageIdResponse>.Error(UserConstants.NameAlreadyUse));
                case false when isPhoneExist:
                    return Ok(ApiResponse<ImageIdResponse>.Error(UserConstants.PhoneAlreadyUse));
                case true:
                    return Ok(ApiResponse<ImageIdResponse>.Error(UserConstants.NameAlreadyUse, UserConstants.PhoneAlreadyUse));
                default:
                {
                    var imageId = request.Image == null
                        ? user.ImageId
                        : string.IsNullOrWhiteSpace(user.ImageId)
                            ? (await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName,
                                Guid.NewGuid().ToString(),
                                request.Image.ContentType, request.Image.OpenReadStream(),
                                cancellationToken: cancellationToken)).Name
                            : (await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName, user.ImageId,
                                request.Image.ContentType, request.Image.OpenReadStream(),
                                cancellationToken: cancellationToken)).Name;

                    await repository.UpdateUser(request.Id, request.Name, request.Phone, imageId, cancellationToken);
                    return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = imageId }));
                }
            }
        }
    }
}