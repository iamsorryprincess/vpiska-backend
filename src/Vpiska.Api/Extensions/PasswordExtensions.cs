using System;
using System.Text;

namespace Vpiska.Api.Extensions
{
    public static class PasswordExtensions
    {
        public static string HashPassword(this string password) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

        public static bool CheckPassword(this string hash, string password) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(password)) == hash;
    }
}