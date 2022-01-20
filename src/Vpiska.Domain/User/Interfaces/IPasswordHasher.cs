namespace Vpiska.Domain.User.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);

        bool VerifyHashPassword(string hashedPassword, string providedPassword);
    }
}