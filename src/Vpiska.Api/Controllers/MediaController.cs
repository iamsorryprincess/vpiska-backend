using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Responses;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Commands.RemoveMediaCommand;
using Vpiska.Domain.Media.Commands.UploadMediaCommand;
using Vpiska.Domain.Media.Models;
using Vpiska.Domain.Media.Queries.GetByNameQuery;
using Vpiska.Domain.Media.Queries.PageQuery;

namespace Vpiska.Api.Controllers
{
    [Route("api/media")]
    public sealed class MediaController : ControllerBase
    {
        [HttpGet("/admin")]
        [Produces("text/html")]
        public IActionResult AdminPage()
        {
            return File("index.html", "text/html");
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse>), 200)]
        public async Task<IActionResult> GetPagedList(
            [FromServices] IQueryHandler<PageQuery, PagedResponse> queryHandler,
            [FromQuery] PageQuery query,
            CancellationToken cancellationToken)
        {
            var data = await queryHandler.HandleAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResponse>.Success(data));
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<MetadataViewModel>), 200)]
        public async Task<IActionResult> PostFile(
            [FromServices] ICommandHandler<UploadMediaCommand, MetadataViewModel> commandHandler,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var body = new byte[file.Length];
            var stream = file.OpenReadStream();
            await stream.ReadAsync(body, cancellationToken);
            await stream.DisposeAsync();
            var command = new UploadMediaCommand()
            {
                Name = file.FileName,
                ContentType = file.ContentType,
                Body = body
            };
            var response = await commandHandler.HandleAsync(command, cancellationToken);
            return Ok(ApiResponse<MetadataViewModel>.Success(response));
        }

        [HttpDelete]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> DeleteFiles(
            [FromServices] ICommandHandler<RemoveMediaCommand> commandHandler,
            [FromForm] string[] names,
            CancellationToken cancellationToken)
        {
            var tasks = names.Select(name =>
                commandHandler.HandleAsync(new RemoveMediaCommand() { Name = name }, cancellationToken));
            await Task.WhenAll(tasks);
            return Ok(ApiResponse.Success());
        }

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