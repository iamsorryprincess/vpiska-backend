using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RtclightWeb.Models;
using RtclightWeb.Services;

namespace RtclightWeb.Controllers
{
    [Route("candidates")]
    public class IceCandidateController : Controller
    {
        private readonly IceCandidateService _candidateService;

        public IceCandidateController(IceCandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetOfferCandidates()
        {
            var iceCandidates = await _candidateService.GetOfferCandidates();
            return Ok(iceCandidates.Select(x => new IceCandidateRequest()
            {
                Candidate = x.Candidate,
                SdpMLineIndex = x.SdpMLineIndex,
                SdpMid = x.SdpMid
            }).ToArray());
        }
        
        [HttpGet("answers")]
        public async Task<IActionResult> GetAnswerCandidates()
        {
            var iceCandidates = await _candidateService.GetAnswerCandidates();
            return Ok(iceCandidates.Select(x => new IceCandidateRequest()
            {
                Candidate = x.Candidate,
                SdpMLineIndex = x.SdpMLineIndex,
                SdpMid = x.SdpMid
            }).ToArray());
        }

        [HttpPost("offers")]
        public async Task<IActionResult> PostOfferCandidate([FromBody] IceCandidateRequest request)
        {
            var model = new IceCandidate()
            {
                Candidate = request.Candidate,
                SdpMLineIndex = request.SdpMLineIndex,
                SdpMid = request.SdpMid
            };
            await _candidateService.InsertOfferCandidate(model);
            return Ok();
        }
        
        [HttpPost("answers")]
        public async Task<IActionResult> PostAnswerCandidate([FromBody] IceCandidateRequest request)
        {
            var model = new IceCandidate()
            {
                Candidate = request.Candidate,
                SdpMLineIndex = request.SdpMLineIndex,
                SdpMid = request.SdpMid
            };
            await _candidateService.InsertAnswerCandidate(model);
            return Ok();
        }
    }
}