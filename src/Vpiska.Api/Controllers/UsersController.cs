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
using Vpiska.Api.Constants;
using Vpiska.Api.Extensions;
using Vpiska.Api.Requests;
using Vpiska.Api.Responses;
using Vpiska.Api.Settings;
using Vpiska.Domain.Models;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private static readonly Random Random = new Random();
        
        private readonly IMongoClient _mongoClient;
        private readonly string _databaseName;

        public UsersController(IMongoClient mongoClient, IOptions<MongoSettings> mongoOptions)
        {
            _mongoClient = mongoClient;
            _databaseName = mongoOptions.Value.DatabaseName;
        }

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public async Task<IActionResult> Create(CancellationToken cancellationToken,
            [FromServices] IValidator<CreateUserRequest> validator,
            [FromBody] CreateUserRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var phoneFilter = request.Phone.CreatePhoneFilter();
            var nameFilter = request.Name.CreateNameFilter();
            var filter = phoneFilter.Or(nameFilter);
            var users = _mongoClient.GetUsers(_databaseName);

            var checks = await users
                .Find(filter)
                .Project(user => new
                    { IsPhoneExist = user.Phone == request.Phone, IsNameExist = user.Name == request.Name })
                .ToListAsync(cancellationToken: cancellationToken);

            var (isPhoneExist, isNameExist) = checks
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
                    return Ok(ApiResponse.Error(UserConstants.NameAlreadyUse));
                case false when isPhoneExist:
                    return Ok(ApiResponse.Error(UserConstants.PhoneAlreadyUse));
                case true:
                    return Ok(ApiResponse.Error(UserConstants.NameAlreadyUse, UserConstants.PhoneAlreadyUse));
                default:
                {
                    var user = new User()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Phone = request.Phone,
                        PhoneCode = UserConstants.PhoneCode,
                        Name = request.Name,
                        Password = request.Password.HashPassword()
                    };

                    await users.InsertOneAsync(user, cancellationToken: cancellationToken);
                    var response = new LoginResponse()
                    {
                        UserId = user.Id,
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
        public async Task<IActionResult> Login(CancellationToken cancellationToken,
            [FromServices] IValidator<LoginUserRequest> validator,
            [FromBody] LoginUserRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var users = _mongoClient.GetUsers(_databaseName);
            var filter = request.Phone.CreatePhoneFilter();
            var user = await users.Find(filter).FirstOrDefaultAsync(cancellationToken);

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
                AccessToken = Jwt.Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };
            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> SetVerificationCode(CancellationToken cancellationToken,
            [FromServices] IValidator<SetCodeRequest> validator,
            [FromBody] SetCodeRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var code = Random.Next(111111, 777777);
            var users = _mongoClient.GetUsers(_databaseName);
            var filter = request.Phone.CreatePhoneFilter();
            var update = Builders<User>.Update.Set(x => x.VerificationCode, code);
            var updateResult = await users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            if (updateResult.MatchedCount < 1)
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
        public async Task<IActionResult> CheckCode(CancellationToken cancellationToken,
            [FromServices] IValidator<CheckCodeRequest> validator,
            [FromBody] CheckCodeRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var users = _mongoClient.GetUsers(_databaseName);
            var filter = request.Phone.CreatePhoneFilter();
            var user = await users.Find(filter).FirstOrDefaultAsync(cancellationToken);

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
                AccessToken = Jwt.Jwt.EncodeJwt(user.Id, user.Name, user.ImageId)
            };
            return Ok(ApiResponse<LoginResponse>.Success(response));
        }

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> ChangePassword(CancellationToken cancellationToken,
            [FromServices] IValidator<ChangePasswordRequest> validator,
            [FromBody] ChangePasswordRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var users = _mongoClient.GetUsers(_databaseName);
            var filter = request.Id.CreateUserIdFilter();
            var update = Builders<User>.Update.Set(x => x.Password, request.Password.HashPassword());
            var updateResult = await users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return Ok(updateResult.MatchedCount < 1 ? ApiResponse.Error(UserConstants.UserNotFound) : ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Update(CancellationToken cancellationToken,
            [FromServices] IValidator<UpdateUserRequest> validator,
            [FromServices] StorageClient fileStorage,
            [FromServices] IOptions<FirebaseSettings> firebaseSettings,
            [FromForm] UpdateUserRequest request)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Phone) &&
                request.Image == null)
            {
                return Ok(ApiResponse.Success());
            }
            
            var users = _mongoClient.GetUsers(_databaseName);
            var filter = request.Id.CreateUserIdFilter();
            var user = await users.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return Ok(ApiResponse.Error(UserConstants.UserNotFound));
            }

            var checks = await Task.WhenAll(
                CheckPhone(users, request.Phone, cancellationToken),
                CheckName(users, request.Name, cancellationToken));

            if (checks[0] && !checks[1])
            {
                return Ok(ApiResponse.Error(UserConstants.PhoneAlreadyUse));
            }

            if (!checks[0] && checks[1])
            {
                return Ok(ApiResponse.Error(UserConstants.NameAlreadyUse));
            }

            if (checks[0] && checks[1])
            {
                return Ok(ApiResponse.Error(UserConstants.PhoneAlreadyUse, UserConstants.NameAlreadyUse));
            }

            string imageId = null;
            
            if (request.Image != null)
            {
                var link = string.IsNullOrWhiteSpace(user.ImageId)
                    ? await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName,
                        Guid.NewGuid().ToString("N"),
                        request.Image.ContentType, request.Image.OpenReadStream(), cancellationToken: cancellationToken)
                    : await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName, user.ImageId,
                        request.Image.ContentType, request.Image.OpenReadStream(),
                        cancellationToken: cancellationToken);
                imageId = link.Name;
            }

            var update = request.CreateUpdateDefinition(imageId);
            await users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return Ok(ApiResponse.Success());
        }
        
        private static async Task<bool> CheckPhone(IMongoCollection<User> users, string phone, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var phoneFilter = phone.CreatePhoneFilter();
            var result = await users.Find(phoneFilter).AnyAsync(cancellationToken);
            return result;
        }

        private static async Task<bool> CheckName(IMongoCollection<User> users, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var nameFilter = name.CreateNameFilter();
            var result = await users.Find(nameFilter).AnyAsync(cancellationToken);
            return result;
        }
    }
}