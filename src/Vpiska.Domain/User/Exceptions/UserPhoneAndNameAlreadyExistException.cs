using System;
using Vpiska.Domain.Common.Exceptions;

namespace Vpiska.Domain.User.Exceptions
{
    [Serializable]
    public sealed class UserPhoneAndNameAlreadyExistException : DomainException
    {
        public UserPhoneAndNameAlreadyExistException() : base(Constants.PhoneAlreadyUse, Constants.NameAlreadyUse)
        {
        }
    }
}