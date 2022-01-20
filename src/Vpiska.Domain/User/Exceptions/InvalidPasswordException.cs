using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class InvalidPasswordException : DomainException
    {
        public InvalidPasswordException() : base(Constants.InvalidPassword)
        {
        }
    }
}