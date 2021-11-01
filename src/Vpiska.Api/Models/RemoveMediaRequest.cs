using System;

namespace Vpiska.Api.Models
{
    public sealed class RemoveMediaRequest
    {
        public Guid? EventId { get; set; }

        public string MediaLink { get; set; }
    }
}