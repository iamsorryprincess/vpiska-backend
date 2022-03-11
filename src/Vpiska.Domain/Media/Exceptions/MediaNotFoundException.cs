using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Media.Exceptions
{
    [Serializable]
    public sealed class MediaNotFoundException : DomainException
    {
        public MediaNotFoundException() : base(Constants.MediaNotFound)
        {
        }
    }
}