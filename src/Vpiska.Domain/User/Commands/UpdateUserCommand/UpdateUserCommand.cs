using System.IO;

namespace Vpiska.Domain.User.Commands.UpdateUserCommand
{
    public sealed class UpdateUserCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string ContentType { get; set; }
        
        public Stream ImageStream { get; set; }
    }
}