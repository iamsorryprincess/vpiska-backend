namespace Vpiska.Domain.Interfaces
{
    public interface IPasswordSecurityService
    {
        string HashPassword(string password);

        bool CheckPassword(string hash, string password);
    }
}