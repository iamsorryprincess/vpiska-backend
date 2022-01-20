using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class UserNameAlreadyExistException : DomainException
    {
        public UserNameAlreadyExistException() : base(Constants.NameAlreadyUse)
        {
        }
    }
}