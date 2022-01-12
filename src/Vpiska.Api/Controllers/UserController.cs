using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using FluentValidation;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Vpiska.Api.Common;
using Vpiska.Api.Constants;
using Vpiska.Api.Identity;
using Vpiska.Api.Models.User;
using Vpiska.Api.Requests.User;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.User;
using Vpiska.Api.Settings;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private static readonly Random Random = new();
        
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Create([FromServices] IValidator<CreateUserRequest> validator,
            [FromServices] DbContext dbContext,
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, request.Phone);
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, request.Name);
            var checkFilter = Builders<User>.Filter.Or(phoneFilter, nameFilter);
            
            var checkResult = await dbContext.Users
                .Find(checkFilter)
                .Project(x => new { IsPhoneExist = x.Phone == request.Phone, IsNameExist = x.Name == request.Name })
                .ToListAsync(cancellationToken: cancellationToken);

            var (isPhoneExist, isNameExist) = checkResult.Aggregate((false, false), (acc, item) =>
            {
                if (item.IsPhoneExist)
                    acc.Item1 = true;
                if (item.IsNameExist)
                    acc.Item2 = true;
                return acc;
            });
            
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
                        Password = PasswordHashing.HashPassword(request.Password)
                    };

                    await dbContext.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
                    
                    var response = new LoginResponse()
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        ImageId = user.ImageId,
                        AccessToken = Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
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
            [FromServices] DbContext dbContext,
            [FromBody] LoginUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var filter = Builders<User>.Filter.Eq(x => x.Phone, request.Phone);
            var user = await dbContext.Users.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserByPhoneNotFound));
            }

            if (!PasswordHashing.CheckPassword(request.Password, user.Password))
            {
                return Ok(ApiResponse.Error(UserConstants.InvalidPassword));
            }

            var response = new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };

            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> SetCode([FromServices] IValidator<SetCodeRequest> validator,
            [FromServices] DbContext dbContext,
            [FromBody] SetCodeRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var code = Random.Next(111111, 777777);
            var filter = Builders<User>.Filter.Eq(x => x.Phone, request.Phone);
            var update = Builders<User>.Update.Set(x => x.VerificationCode, code);
            var result = await dbContext.Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
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
            [FromServices] DbContext dbContext,
            [FromBody] CheckCodeRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var filter = Builders<User>.Filter.Eq(x => x.Phone, request.Phone);
            var user = await dbContext.Users.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserByPhoneNotFound));
            }

            if (user.VerificationCode != request.Code.Value)
            {
                return Ok(ApiResponse.Error(UserConstants.InvalidCode));
            }
            
            var response = new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };
            
            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> ChangePassword([FromServices] IValidator<ChangePasswordRequest> validator,
            [FromServices] DbContext dbContext,
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var filter = Builders<User>.Filter.Eq(x => x.Id, request.Id);
            var update = Builders<User>.Update.Set(x => x.Password, PasswordHashing.HashPassword(request.Password));
            var result = await dbContext.Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            var response = result.MatchedCount > 0
                ? ApiResponse.Success()
                : ApiResponse.Error(UserConstants.UserNotFound);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImageIdResponse>), 200)]
        public async Task<IActionResult> Update([FromServices] IValidator<UpdateUserRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] StorageClient storageClient,
            [FromServices] IOptions<FirebaseSettings> firebaseOptions,
            [FromForm] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }
            
            if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Phone) &&
                request.Image == null)
            {
                return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = null }));
            }

            var filter = Builders<User>.Filter.Eq(x => x.Id, request.Id);
            var user = await dbContext.Users.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserNotFound));
            }
            
            var isNameEmpty = string.IsNullOrWhiteSpace(request.Name);
            var isPhoneEmpty = string.IsNullOrWhiteSpace(request.Phone);
            var checkFilter = isNameEmpty switch
            {
                true when isPhoneEmpty => null,
                false when isPhoneEmpty => Builders<User>.Filter.Eq(x => x.Name, request.Name),
                true => Builders<User>.Filter.Eq(x => x.Phone, request.Phone),
                _ => Builders<User>.Filter.Or(Builders<User>.Filter.Eq(x => x.Name, request.Name),
                    Builders<User>.Filter.Eq(x => x.Phone, request.Phone))
            };

            var (isPhoneExist, isNameExist) = checkFilter == null
                ? (false, false)
                : (await dbContext.Users
                    .Find(checkFilter)
                    .Project(x => new { IsPhoneExist = x.Phone == request.Phone, IsNameExist = x.Name == request.Name })
                    .ToListAsync(cancellationToken: cancellationToken))
                .Aggregate((false, false), (acc, item) =>
                {
                    if (item.IsPhoneExist)
                        acc.Item1 = true;
                    if (item.IsNameExist)
                        acc.Item2 = true;
                    return acc;
                });
            
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
                            ? (await storageClient.UploadObjectAsync(firebaseOptions.Value.BucketName,
                                Guid.NewGuid().ToString(),
                                request.Image.ContentType, request.Image.OpenReadStream(),
                                cancellationToken: cancellationToken)).Name
                            : (await storageClient.UploadObjectAsync(firebaseOptions.Value.BucketName, user.ImageId,
                                request.Image.ContentType, request.Image.OpenReadStream(),
                                cancellationToken: cancellationToken)).Name;

                    var update = isNameEmpty switch
                    {
                        true when isPhoneEmpty => Builders<User>.Update.Set(x => x.ImageId, imageId),
                        false when isPhoneEmpty => Builders<User>.Update.Combine(
                            Builders<User>.Update.Set(x => x.Name, request.Name),
                            Builders<User>.Update.Set(x => x.ImageId, imageId)),
                        true => Builders<User>.Update.Combine(Builders<User>.Update.Set(x => x.Phone, request.Phone),
                            Builders<User>.Update.Set(x => x.ImageId, imageId)),
                        _ => Builders<User>.Update.Combine(Builders<User>.Update.Set(x => x.Name, request.Name),
                            Builders<User>.Update.Set(x => x.Phone, request.Phone),
                            Builders<User>.Update.Set(x => x.ImageId, imageId))
                    };

                    await dbContext.Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
                    return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = imageId }));
                }
            }
        }
    }
}