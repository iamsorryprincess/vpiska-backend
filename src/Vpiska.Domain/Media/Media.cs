using System;

namespace Vpiska.Domain.Media
{
    public sealed class Media
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public string Extension { get; set; }

        public int Size { get; set; }
        
        public DateTime LastModifiedDate { get; set; }
    }
}