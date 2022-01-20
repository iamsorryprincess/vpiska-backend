using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class UserPhoneAlreadyExistException : DomainException
    {
        public UserPhoneAlreadyExistException() : base(Constants.PhoneAlreadyUse)
        {
        }
    }
}