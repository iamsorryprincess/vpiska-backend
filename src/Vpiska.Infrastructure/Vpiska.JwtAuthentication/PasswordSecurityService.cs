using System;
using System.Text;
using Vpiska.Domain.Interfaces;

namespace Vpiska.JwtAuthentication
{
    internal sealed class PasswordSecurityService : IPasswordSecurityService
    {
        public string HashPassword(string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

        public bool CheckPassword(string hash, string password) =>
            hash == Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }
}