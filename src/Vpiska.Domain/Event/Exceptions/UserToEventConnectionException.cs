using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Event.Exceptions
{
    [Serializable]
    public sealed class UserToEventConnectionException : DomainException
    {
    }
}