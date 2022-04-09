using System.ComponentModel.DataAnnotations;

namespace RtclightWeb.Models
{
    public sealed class IceCandidateRequest
    {
        [Required]
        public string Candidate { get; set; }

        [Required]
        public int SdpMLineIndex { get; set; }

        [Required]
        public string SdpMid { get; set; }
    }
}