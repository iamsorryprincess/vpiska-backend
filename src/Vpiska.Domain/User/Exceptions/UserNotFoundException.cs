using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class UserNotFoundException : DomainException
    {
        public UserNotFoundException(params string[] errorCodes) : base(errorCodes)
        {
        }
    }
}