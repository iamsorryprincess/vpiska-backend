using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Responses;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Commands.UploadMediaCommand;
using Vpiska.Domain.Media.Interfaces;
using Vpiska.Domain.Media.Models;
using Vpiska.Domain.Media.Queries.GetByNameQuery;

namespace Vpiska.Api.Controllers
{
    [Route("api/media")]
    public sealed class MediaController : Controller
    {
        [HttpGet("/admin")]
        [Produces("text/html")]
        public async Task<IActionResult> AdminPage([FromServices] IMediaRepository repository,
            int page = 1,
            int size = 20)
        {
            var filesMetadata = await repository.GetPagedFilesMetadata(page, size);
            return View("~/Views/Admin/Admin.cshtml", filesMetadata);
        }

        [HttpPost]
        [Produces("text/html")]
        public async Task<IActionResult> UploadFileForm(
            [FromServices] ICommandHandler<UploadMediaCommand> commandHandler,
            IFormFile file,
            int page,
            int size,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[file.Length];
            var stream = file.OpenReadStream();
            await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            var command = new UploadMediaCommand()
            {
                Name = file.FileName,
                ContentType = file.ContentType,
                Body = buffer
            };
            await commandHandler.HandleAsync(command, cancellationToken);
            return RedirectToAction("AdminPage", new { page, size });
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