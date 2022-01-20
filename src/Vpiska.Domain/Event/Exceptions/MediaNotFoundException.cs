using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Event.Exceptions
{
    public sealed class MediaNotFoundException : DomainException
    {
        public MediaNotFoundException() : base(Constants.MediaNotFound)
        {
        }
    }
}