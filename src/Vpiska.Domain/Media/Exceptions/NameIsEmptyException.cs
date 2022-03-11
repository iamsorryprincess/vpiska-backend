using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Media.Exceptions
{
    [Serializable]
    public sealed class NameIsEmptyException : DomainException
    {
        public NameIsEmptyException() : base(Constants.NameIsEmpty)
        {
        }
    }
}