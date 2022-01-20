using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.Event.Exceptions
{
    [Serializable]
    public sealed class UserIsNotOwnerException : DomainException
    {
        public UserIsNotOwnerException() : base(Constants.UserIsNotOwner)
        {
        }
    }
}