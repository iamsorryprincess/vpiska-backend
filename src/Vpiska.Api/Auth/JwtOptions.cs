using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Vpiska.Api.Auth
{
    internal static class JwtOptions
    {
        private const string Key = "vpiska_secretkey!123";
        
        public const string Issuer = "VpiskaServer";
        public const string Audience = "VpiskaClient";

        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}