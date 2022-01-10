using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Google;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orleans;
using Vpiska.Api.Common;
using Vpiska.Api.Constants;
using Vpiska.Api.Models.Event;
using Vpiska.Api.Orleans;
using Vpiska.Api.Requests.Event;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.Event;
using Vpiska.Api.Settings;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventController : ControllerBase
    {
        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> Create([FromServices] IValidator<CreateEventRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] IClusterClient clusterClient,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var ownerId = GetUserId();
            var ownerIdFilter = Builders<Event>.Filter.Eq(x => x.OwnerId, ownerId);
            var isOwnerHasEvent = await dbContext.Events.Find(ownerIdFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isOwnerHasEvent)
            {
                return Ok(ApiResponse.Error(EventConstants.OwnerAlreadyHasEvent));
            }

            var model = new Event()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                Name = request.Name,
                Address = request.Address,
                Coordinates = new Coordinates()
                {
                    X = request.Coordinates.X.Value,
                    Y = request.Coordinates.Y.Value
                }
            };

            await dbContext.Events.InsertOneAsync(model, cancellationToken: cancellationToken);
            var grain = clusterClient.GetGrain<IEventGrain>(model.Id);
            await grain.Init(model);
            return Ok(ApiResponse<Event>.Success(model));
        }
        
        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> GetById([FromServices] IValidator<IdRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] IClusterClient clusterClient,
            [FromBody] IdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var grain = clusterClient.GetGrain<IEventGrain>(request.Id);
            var model = await grain.GetData();

            if (model != null)
            {
                return Ok(ApiResponse<Event>.Success(model));
            }

            var filter = Builders<Event>.Filter.Eq(x => x.Id, request.Id);
            model = await dbContext.Events.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (model == null)
            {
                return Ok(ApiResponse.Error(EventConstants.EventNotFound));
            }

            await grain.SetData(model);
            return Ok(ApiResponse<Event>.Success(model));
        }

        [HttpPost("all")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventShortResponse[]>), 200)]
        public async Task<IActionResult> GetAll([FromServices] IValidator<GetEventsRequest> validator,
            [FromServices] DbContext dbContext,
            [FromBody] GetEventsRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var halfHorizontalRange = request.HorizontalRange.Value / 2;
            var halfVerticalRange = request.VerticalRange.Value / 2;
            var xLeft = request.Coordinates.X.Value - halfHorizontalRange;
            var xRight = request.Coordinates.X.Value + halfHorizontalRange;
            var yLeft = request.Coordinates.Y.Value - halfVerticalRange;
            var yRight = request.Coordinates.Y.Value + halfVerticalRange;
            var leftHorizontalFilter = Builders<Event>.Filter.Gte(x => x.Coordinates.X, xLeft);
            var rightHorizontalFilter = Builders<Event>.Filter.Lte(x => x.Coordinates.X, xRight);
            var leftVerticalFilter = Builders<Event>.Filter.Gte(x => x.Coordinates.Y, yLeft);
            var rightVerticalFilter = Builders<Event>.Filter.Lte(x => x.Coordinates.Y, yRight);
            var filter = Builders<Event>.Filter.And(leftHorizontalFilter, rightHorizontalFilter, leftVerticalFilter,
                rightVerticalFilter);

            var result = await dbContext.Events.Find(filter).Project(data => new EventShortResponse()
            {
                Id = data.Id,
                Name = data.Name,
                Coordinates = data.Coordinates,
                UsersCount = data.Users.Count
            }).ToListAsync(cancellationToken: cancellationToken);

            return Ok(ApiResponse<List<EventShortResponse>>.Success(result));
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Close([FromServices] IValidator<IdRequest> validator,
            [FromServices] IClusterClient clusterClient,
            [FromServices] DbContext dbContext,
            [FromBody] IdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var grain = clusterClient.GetGrain<IEventGrain>(request.Id);
            var model = await grain.GetData();

            if (model == null)
            {
                var filter = Builders<Event>.Filter.Eq(x => x.Id, request.Id);
                model = await dbContext.Events.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (model == null)
                {
                    return Ok(ApiResponse.Error(EventConstants.EventNotFound));
                }
            }

            if (model.OwnerId != GetUserId())
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            await grain.Close();
            var idFilter = Builders<Event>.Filter.Eq(x => x.Id, request.Id);
            await dbContext.Events.DeleteOneAsync(idFilter, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<MediaResponse>), 200)]
        public async Task<IActionResult> AddMedia([FromServices] IValidator<AddMediaRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] IClusterClient clusterClient,
            [FromServices] StorageClient storageClient,
            [FromServices] IOptions<FirebaseSettings> firebaseOptions,
            [FromForm] AddMediaRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var grain = clusterClient.GetGrain<IEventGrain>(request.EventId);
            var model = await grain.GetData();

            if (model == null)
            {
                var idFilter = Builders<Event>.Filter.Eq(x => x.Id, request.EventId);
                model = await dbContext.Events.Find(idFilter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (model == null)
                {
                    return Ok(ApiResponse.Error(EventConstants.EventNotFound));
                }
            }

            if (model.OwnerId != GetUserId())
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            var uploadResult = await storageClient.UploadObjectAsync(firebaseOptions.Value.BucketName,
                Guid.NewGuid().ToString(), request.Media.ContentType, request.Media.OpenReadStream(),
                cancellationToken: cancellationToken);

            var filter = Builders<Event>.Filter.Eq(x => x.Id, request.EventId);
            var update = Builders<Event>.Update.AddToSet(x => x.MediaLinks, uploadResult.Name);
            await dbContext.Events.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            await grain.AddMedia(uploadResult.Name);
            return Ok(ApiResponse<MediaResponse>.Success(new MediaResponse() { MediaId = uploadResult.Name }));
        }

        
        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> RemoveMedia([FromServices] IValidator<RemoveMediaRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] IClusterClient clusterClient,
            [FromServices] StorageClient storageClient,
            [FromServices] IOptions<FirebaseSettings> firebaseOptions,
            [FromBody] RemoveMediaRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorResponse = ApiResponse.Error(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                return Ok(errorResponse);
            }

            var grain = clusterClient.GetGrain<IEventGrain>(request.EventId);
            var model = await grain.GetData();

            if (model == null)
            {
                var idFilter = Builders<Event>.Filter.Eq(x => x.Id, request.EventId);
                model = await dbContext.Events.Find(idFilter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (model == null)
                {
                    return Ok(ApiResponse.Error(EventConstants.EventNotFound));
                }
            }

            if (model.OwnerId != GetUserId())
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            if (!model.MediaLinks.Contains(request.MediaId))
            {
                return Ok(ApiResponse.Error(EventConstants.MediaNotFound));
            }

            try
            {
                await storageClient.DeleteObjectAsync(firebaseOptions.Value.BucketName, request.MediaId,
                    cancellationToken: cancellationToken);
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            var filter = Builders<Event>.Filter.Eq(x => x.Id, request.EventId);
            var update = Builders<Event>.Update.Pull(x => x.MediaLinks, request.MediaId);
            await dbContext.Events.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            await grain.RemoveMedia(request.MediaId);
            return Ok(ApiResponse.Success());
        }

        private string GetUserId()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParse(userId.Value, out _))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return userId.Value;
        }
    }
}