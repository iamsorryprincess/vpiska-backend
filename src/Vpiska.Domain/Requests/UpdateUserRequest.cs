using System.IO;

namespace Vpiska.Domain.Requests
{
    public sealed class UpdateUserRequest
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Phone { get; set; }

        public Stream ImageStream { get; set; }
    }
}