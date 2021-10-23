using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Domain.Base;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public Task<DomainResponse<LoginResponse>> Create([FromBody] CreateUserRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [HttpPost("login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public Task<DomainResponse<LoginResponse>> Login([FromBody] LoginUserRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [HttpPost("code/set")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public Task<DomainResponse> SetCode([FromBody] SetCodeRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [HttpPost("code/check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse<LoginResponse>), 200)]
        public Task<DomainResponse<LoginResponse>> CheckCode([FromBody] CheckCodeRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [Authorize]
        [HttpPost("password/change")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public Task<DomainResponse> ChangePassword([FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken) => _mediator.Send(request, cancellationToken);

        [Authorize]
        [HttpPost("update")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DomainResponse), 200)]
        public async Task<DomainResponse> UpdateUser([FromForm] Models.UpdateUserRequest formRequest,
            CancellationToken cancellationToken)
        {
            var request = new UpdateUserRequest()
            {
                Id = formRequest.Id,
                Name = formRequest.Name,
                Phone = formRequest.Phone
            };

            if (formRequest.Image != null)
            {
                await using var stream = new MemoryStream();
                await formRequest.Image.CopyToAsync(stream, cancellationToken);
                request.ImageData = stream.ToArray();
                request.ContentType = formRequest.Image.ContentType;
            }

            var response = await _mediator.Send(request, cancellationToken);
            return response;
        }
    }
}