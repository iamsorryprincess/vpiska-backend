using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Media.Exceptions
{
    [Serializable]
    public sealed class ContentTypeNotSupportedException : DomainException
    {
        public ContentTypeNotSupportedException() : base(Constants.ContentTypeNotSupported)
        {
        }
    }
}