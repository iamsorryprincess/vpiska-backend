using Vpiska.Domain.User.Interfaces;
using Vpiska.Infrastructure.Identity;
using Xunit;

namespace Vpiska.UnitTests.User
{
    public sealed class PasswordHashingTests
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHashingTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void Test()
        {
            const string password = "test";
            var hashedPassword = _passwordHasher.HashPassword(password);
            var isSuccess = _passwordHasher.VerifyHashPassword(hashedPassword, password);
            Assert.True(isSuccess);
            Assert.True(_passwordHasher.VerifyHashPassword(hashedPassword, "test"));
            Assert.False(_passwordHasher.VerifyHashPassword(hashedPassword, "wrong"));
        }
    }
}