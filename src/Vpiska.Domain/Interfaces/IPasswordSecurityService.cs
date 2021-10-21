namespace Vpiska.Domain.Interfaces
{
    public interface IPasswordSecurityService
    {
        string HashPassword(string password);

        bool IsPasswordInvalid(string hash, string password);
    }
}