using System.Collections.Generic;

namespace Vpiska.Domain.Media.Models
{
    public sealed class PagedResponse
    {
        public int Page { get; set; }

        public int ItemsPerPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }

        public List<MetadataViewModel> FilesMetadata { get; set; }
    }
}