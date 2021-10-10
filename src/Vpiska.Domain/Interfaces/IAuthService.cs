namespace Vpiska.Domain.Interfaces
{
    public interface IAuthService
    {
        string GetEncodedJwt(string userId, string username, string imageUrl);

        string HashPassword(string password);

        bool CheckPassword(string hash, string password);
    }
}