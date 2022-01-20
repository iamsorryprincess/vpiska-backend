using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Event.Exceptions
{
    [Serializable]
    public sealed class OwnerAlreadyHasEventException : DomainException
    {
        public OwnerAlreadyHasEventException() : base(Constants.OwnerAlreadyHasEvent)
        {
        }
    }
}