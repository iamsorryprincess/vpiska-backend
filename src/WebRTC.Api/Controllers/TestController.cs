using Microsoft.AspNetCore.Mvc;

namespace WebRTC.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class TestController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Get() => File("index.html", "text/html");
}