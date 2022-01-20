using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class InvalidCodeException : DomainException
    {
        public InvalidCodeException() : base(Constants.InvalidCode)
        {
        }
    }
}