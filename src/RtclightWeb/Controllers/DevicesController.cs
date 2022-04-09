using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RtclightWeb.Models;
using RtclightWeb.Services;

namespace RtclightWeb.Controllers
{
    [Authorize]
    public class DevicesController : Controller
    {
        private readonly OfferService _offerService;

        public DevicesController(OfferService offerService)
        {
            _offerService = offerService;
        }
        
        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        [Route("devices/offers")]
        public async Task<IActionResult> GetLastOffer()
        {
            var offer = await _offerService.GetLast();
            return Ok(new OfferDto()
            {
                Type = offer.Type,
                Sdp = offer.Sdp
            });
        }
    }
}