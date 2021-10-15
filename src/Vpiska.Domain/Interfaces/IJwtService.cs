using System;

namespace Vpiska.Domain.Interfaces
{
    public interface IJwtService
    {
        string EncodeJwt(Guid userId, string username, string imageId);
    }
}