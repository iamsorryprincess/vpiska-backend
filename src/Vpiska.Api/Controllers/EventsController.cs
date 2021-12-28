using System;
using System.Collections.Generic;
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
using Vpiska.Api.Constants;
using Vpiska.Api.Extensions;
using Vpiska.Api.Requests.Event;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.Event;
using Vpiska.Api.Settings;
using Vpiska.Domain.Models;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly IMongoClient _mongoClient;
        private readonly string _databaseName;

        public EventsController(IMongoClient mongoClient, IOptions<MongoSettings> mongoOptions)
        {
            _mongoClient = mongoClient;
            _databaseName = mongoOptions.Value.DatabaseName;
        }

        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> Create([FromServices] IValidator<CreateEventRequest> validator,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var events = _mongoClient.GetEvents(_databaseName);
            var ownerId = HttpContext.GetUserId();
            var filter = Builders<Event>.Filter.Eq(x => x.OwnerId, ownerId);
            var isOwnerHasEvent = await events.Find(filter).AnyAsync(cancellationToken: cancellationToken);

            if (isOwnerHasEvent)
            {
                return Ok(ApiResponse<Event>.Error(EventConstants.OwnerAlreadyHasEvent));
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
            
            await events.InsertOneAsync(model, cancellationToken: cancellationToken);
            return Ok(ApiResponse<Event>.Success(model));
        }
        
        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> Get([FromServices] IValidator<EventIdRequest> validator,
            [FromBody] EventIdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var events = _mongoClient.GetEvents(_databaseName);
            var filter = request.EventId.CreateEventIdFilter();
            var eventData = await events.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return Ok(eventData == null
                ? ApiResponse<Event>.Error(EventConstants.EventNotFound)
                : ApiResponse<Event>.Success(eventData));
        }

        [HttpPost("all")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventShortResponse[]>), 200)]
        public async Task<IActionResult> GetAll([FromServices] IValidator<GetEventsRequest> validator,
            [FromBody] GetEventsRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var events = _mongoClient.GetEvents(_databaseName);
            var xLeft = request.Coordinates.X.Value - request.Range.Value;
            var xRight = request.Coordinates.X.Value + request.Range.Value;
            var yBottom = request.Coordinates.Y.Value - request.Range.Value;
            var yTop = request.Coordinates.Y.Value + request.Range.Value;
            var filter = Builders<Event>.Filter
                .Where(x => x.Coordinates.X >= xLeft && x.Coordinates.X <= xRight &&
                            x.Coordinates.Y >= yBottom && x.Coordinates.Y <= yTop);

            var response = await events.Find(filter)
                .Project(x => new EventShortResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Coordinates = x.Coordinates,
                    UsersCount = x.Users.Count
                })
                .ToListAsync(cancellationToken: cancellationToken);

            return Ok(ApiResponse<List<EventShortResponse>>.Success(response));
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Close([FromServices] IValidator<EventIdRequest> validator,
            [FromBody] EventIdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var ownerId = HttpContext.GetUserId();
            var events = _mongoClient.GetEvents(_databaseName);
            var idFilter = request.EventId.CreateEventIdFilter();
            var isEventNotExist = !await events.Find(idFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse.Error(EventConstants.EventNotFound));
            }

            var ownerFilter = Builders<Event>.Filter.Eq(x => x.OwnerId, ownerId).And(idFilter);
            var isWrongOwner = !await events.Find(ownerFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            await events.DeleteOneAsync(idFilter, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImageIdResponse>), 200)]
        public async Task<IActionResult> AddMedia([FromServices] IValidator<AddMediaRequest> validator,
            [FromServices] StorageClient fileStorage,
            [FromServices] IOptions<FirebaseSettings> firebaseSettings,
            [FromForm] AddMediaRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var events = _mongoClient.GetEvents(_databaseName);
            var filter = request.EventId.CreateEventIdFilter();
            var isEventNotExist = !await events.Find(filter).AnyAsync(cancellationToken: cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse<ImageIdResponse>.Error(EventConstants.EventNotFound));
            }

            var ownerFilter = Builders<Event>.Filter.Eq(x => x.OwnerId, HttpContext.GetUserId()).And(filter);
            var isWrongOwner = !await events.Find(ownerFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse<ImageIdResponse>.Error(EventConstants.UserIsNotOwner));
            }

            var image = await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName,
                Guid.NewGuid().ToString(),
                request.Media.ContentType, request.Media.OpenReadStream(), cancellationToken: cancellationToken);

            var update = Builders<Event>.Update.AddToSet(x => x.MediaLinks, image.Name);
            await events.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = image.Name }));
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> RemoveMedia([FromServices] IValidator<RemoveMediaRequest> validator,
            [FromServices] StorageClient fileStorage,
            [FromServices] IOptions<FirebaseSettings> firebaseSettings,
            [FromBody] RemoveMediaRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var events = _mongoClient.GetEvents(_databaseName);
            var filter = request.EventId.CreateEventIdFilter();
            var isEventNotExist = !await events.Find(filter).AnyAsync(cancellationToken: cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse.Error(EventConstants.EventNotFound));
            }

            var ownerFilter = Builders<Event>.Filter.Eq(x => x.OwnerId, HttpContext.GetUserId()).And(filter);
            var isWrongOwner = !await events.Find(ownerFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            var mediaIdFilter = Builders<Event>.Filter.AnyEq(x => x.MediaLinks, request.MediaId).And(filter);
            var isMediaNotExist = !await events.Find(mediaIdFilter).AnyAsync(cancellationToken: cancellationToken);

            if (isMediaNotExist)
            {
                return Ok(ApiResponse.Error(EventConstants.MediaNotFound));
            }

            try
            {
                await fileStorage.DeleteObjectAsync(firebaseSettings.Value.BucketName, request.MediaId,
                    cancellationToken: cancellationToken);
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            var update = Builders<Event>.Update.Pull(x => x.MediaLinks, request.MediaId);
            await events.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return Ok(ApiResponse.Success());
        }
    }
}