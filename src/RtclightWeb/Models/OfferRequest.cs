using System.ComponentModel.DataAnnotations;

namespace RtclightWeb.Models
{
    public sealed class OfferDto
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Sdp { get; set; }
    }
}