using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vpiska.Api.Requests.Event;
using Vpiska.Api.Responses;

namespace Vpiska.Api.Controllers
{
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        [HttpPost("create")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public IActionResult Create([FromBody] CreateEventRequest request)
        {
            return Ok();
        }
    }
}