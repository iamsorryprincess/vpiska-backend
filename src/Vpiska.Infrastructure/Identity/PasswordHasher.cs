using System;
using System.Text;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Infrastructure.Identity
{
    internal sealed class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

        public bool VerifyHashPassword(string hashedPassword, string providedPassword) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(providedPassword)) == hashedPassword;
    }
}