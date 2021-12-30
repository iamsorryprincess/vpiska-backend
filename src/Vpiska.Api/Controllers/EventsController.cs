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
using Vpiska.Api.Constants;
using Vpiska.Api.Extensions;
using Vpiska.Api.Requests.Event;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.Event;
using Vpiska.Api.Settings;
using Vpiska.Domain.Models;
using Vpiska.Mongo;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> Create([FromServices] IValidator<CreateEventRequest> validator,
            [FromServices] IEventRepository repository,
            [FromBody] CreateEventRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var ownerId = HttpContext.GetUserId();
            var isOwnerHasEvent = await repository.CheckAsync(x => x.OwnerId, ownerId, cancellationToken);

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

            await repository.InsertAsync(model, cancellationToken);
            return Ok(ApiResponse<Event>.Success(model));
        }
        
        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> Get([FromServices] IValidator<EventIdRequest> validator,
            [FromServices] IEventRepository repository,
            [FromBody] EventIdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }

            var eventData = await repository.GetAsync(x => x.Id, request.EventId, cancellationToken);

            return Ok(eventData == null
                ? ApiResponse<Event>.Error(EventConstants.EventNotFound)
                : ApiResponse<Event>.Success(eventData));
        }

        [HttpPost("all")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<EventShortResponse[]>), 200)]
        public async Task<IActionResult> GetAll([FromServices] IValidator<GetEventsRequest> validator,
            [FromServices] IEventRepository repository,
            [FromBody] GetEventsRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var xLeft = request.Coordinates.X.Value - request.Range.Value;
            var xRight = request.Coordinates.X.Value + request.Range.Value;
            var yBottom = request.Coordinates.Y.Value - request.Range.Value;
            var yTop = request.Coordinates.Y.Value + request.Range.Value;

            var response = await repository.WhereProjectListAsync(x =>
                    x.Coordinates.X >= xLeft && x.Coordinates.X <= xRight &&
                    x.Coordinates.Y >= yBottom && x.Coordinates.Y <= yTop,
                x => new EventShortResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Coordinates = x.Coordinates,
                    UsersCount = x.Users.Count
                }, cancellationToken);

            return Ok(ApiResponse<List<EventShortResponse>>.Success(response));
        }

        [Authorize]
        [HttpPost("close")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Close([FromServices] IValidator<EventIdRequest> validator,
            [FromServices] IEventRepository repository,
            [FromBody] EventIdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Ok(validationResult.MapToResponse());
            }
            
            var isEventNotExist = !await repository.CheckAsync(x => x.Id, request.EventId, cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse.Error(EventConstants.EventNotFound));
            }

            var isWrongOwner =
                !await repository.CheckForOwnershipAsync(request.EventId, HttpContext.GetUserId(), cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }

            await repository.DeleteAsync(x => x.Id, request.EventId, cancellationToken);
            return Ok(ApiResponse.Success());
        }

        [Authorize]
        [HttpPost("media/add")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ImageIdResponse>), 200)]
        public async Task<IActionResult> AddMedia([FromServices] IValidator<AddMediaRequest> validator,
            [FromServices] IEventRepository repository,
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
            
            var isEventNotExist = !await repository.CheckAsync(x => x.Id, request.EventId, cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse<ImageIdResponse>.Error(EventConstants.EventNotFound));
            }

            var isWrongOwner =
                !await repository.CheckForOwnershipAsync(request.EventId, HttpContext.GetUserId(), cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse<ImageIdResponse>.Error(EventConstants.UserIsNotOwner));
            }

            var image = await fileStorage.UploadObjectAsync(firebaseSettings.Value.BucketName,
                Guid.NewGuid().ToString(),
                request.Media.ContentType, request.Media.OpenReadStream(), cancellationToken: cancellationToken);

            await repository.AddMediaAsync(request.EventId, image.Name, cancellationToken);
            return Ok(ApiResponse<ImageIdResponse>.Success(new ImageIdResponse() { ImageId = image.Name }));
        }

        [Authorize]
        [HttpPost("media/remove")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> RemoveMedia([FromServices] IValidator<RemoveMediaRequest> validator,
            [FromServices] IEventRepository repository,
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

            var isEventNotExist = !await repository.CheckAsync(x => x.Id, request.EventId, cancellationToken);

            if (isEventNotExist)
            {
                return Ok(ApiResponse.Error(EventConstants.EventNotFound));
            }

            var isWrongOwner =
                !await repository.CheckForOwnershipAsync(request.EventId, HttpContext.GetUserId(), cancellationToken);

            if (isWrongOwner)
            {
                return Ok(ApiResponse.Error(EventConstants.UserIsNotOwner));
            }
            
            var isMediaNotExist = !await repository.CheckMediaAsync(request.EventId, request.MediaId, cancellationToken);

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

            await repository.DeleteMediaAsync(request.EventId, request.MediaId, cancellationToken);
            return Ok(ApiResponse.Success());
        }
    }
}