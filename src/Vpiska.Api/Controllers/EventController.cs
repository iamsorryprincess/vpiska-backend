using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Orleans;
using Vpiska.Api.Common;
using Vpiska.Api.Constants;
using Vpiska.Api.Models.Event;
using Vpiska.Api.Orleans;
using Vpiska.Api.Requests.Event;
using Vpiska.Api.Responses;
using Vpiska.Api.Responses.Event;

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
            await grain.SetData(model);
            return Ok(ApiResponse<Event>.Success(model));
        }
        
        [HttpPost("single")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Event>), 200)]
        public async Task<IActionResult> GetById([FromServices] IValidator<GetByIdRequest> validator,
            [FromServices] DbContext dbContext,
            [FromServices] IClusterClient clusterClient,
            [FromBody] GetByIdRequest request,
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