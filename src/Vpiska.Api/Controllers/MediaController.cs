using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Responses;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Models;
using Vpiska.Domain.Media.Queries.GetByNameQuery;

namespace Vpiska.Api.Controllers
{
    [Route("api/media")]
    public sealed class MediaController : ControllerBase
    {
        [HttpGet("metadata/{name}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<MetadataResponse>), 200)]
        public async Task<IActionResult> GetMetadata([FromServices] IQueryHandler<GetByNameQuery, Media> queryHandler,
            string name,
            CancellationToken cancellationToken)
        {
            var query = new GetByNameQuery() { Name = name };
            var media = await queryHandler.HandleAsync(query, cancellationToken);
            return Ok(ApiResponse<MetadataResponse>.Success(MetadataResponse.FromModel(media)));
        }

        [HttpGet("{name}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get([FromServices] IQueryHandler<GetByNameQuery, Media> queryHandler,
            string name,
            CancellationToken cancellationToken)
        {
            var query = new GetByNameQuery() { Name = name };
            var media = await queryHandler.HandleAsync(query, cancellationToken);
            return File(new FileStream($"{Constants.Path}/{media.Name}.{media.Extension}", FileMode.Open),
                media.ContentType);
        }
    }
}