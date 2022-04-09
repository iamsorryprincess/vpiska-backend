using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RtclightWeb.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}