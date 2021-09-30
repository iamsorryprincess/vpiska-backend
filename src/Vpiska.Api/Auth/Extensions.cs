using System;
using System.Text;

namespace Vpiska.Api.Auth
{
    internal static class Extensions
    {
        public static string HashPassword(this string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        public static bool CheckHashPassword(this string hash, string password)
        {
            return hash == Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }
    }
}