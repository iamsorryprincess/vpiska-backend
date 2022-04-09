using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RtclightWeb.Models;
using RtclightWeb.Services;

namespace RtclightWeb.Controllers
{
    [Authorize]
    public class ConnectController : Controller
    {
        private readonly OfferService _offerService;

        public ConnectController(OfferService offerService)
        {
            _offerService = offerService;
        }
        
        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [Route("connect")]
        public async Task<IActionResult> Post([FromBody]OfferDto request)
        {
            var offer = new Offer()
            {
                Type = request.Type,
                Sdp = request.Sdp
            };

            var id = await _offerService.Insert(offer);
            return Ok(id);
        }
    }
}