namespace Vpiska.Domain.User.Interfaces
{
    public interface IIdentityService
    {
        string GetAccessToken(User user);
    }
}