using System;
using System.Text;

namespace Vpiska.Api.Identity
{
    public static class PasswordHashing
    {
        public static string HashPassword(string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

        public static bool CheckPassword(string password, string hash) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(password)) == hash;
    }
}