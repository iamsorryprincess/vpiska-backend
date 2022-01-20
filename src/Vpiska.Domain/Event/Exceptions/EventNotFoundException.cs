using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Event.Exceptions
{
    [Serializable]
    public sealed class EventNotFoundException : DomainException
    {
        public EventNotFoundException() : base(Constants.EventNotFound)
        {
        }
    }
}