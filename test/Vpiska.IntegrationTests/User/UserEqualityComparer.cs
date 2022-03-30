using System;
using System.Collections.Generic;

namespace Vpiska.IntegrationTests.User
{
    public sealed class UserEqualityComparer : IEqualityComparer<Domain.User.User>
    {
        public bool Equals(Domain.User.User? x, Domain.User.User? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id && x.Name == y.Name && x.PhoneCode == y.PhoneCode && x.Phone == y.Phone &&
                   x.ImageId == y.ImageId && x.Password == y.Password && x.VerificationCode == y.VerificationCode;
        }

        public int GetHashCode(Domain.User.User obj)
        {
            return HashCode.Combine(
                obj.Id,
                obj.Name,
                obj.PhoneCode,
                obj.Phone,
                obj.ImageId,
                obj.Password,
                obj.VerificationCode);
        }
    }
}