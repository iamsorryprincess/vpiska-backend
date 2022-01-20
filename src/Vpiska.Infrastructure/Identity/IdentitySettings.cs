using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Vpiska.Infrastructure.Identity
{
    internal sealed class IdentitySettings
    {
        public string Key { get; }
        
        public string Issuer { get; }
        
        public string Audience { get; }
        
        public int LifetimeDays { get; }

        public IdentitySettings(string key, string issuer, string audience, int lifetimeDays)
        {
            Key = key;
            Issuer = issuer;
            Audience = audience;
            LifetimeDays = lifetimeDays;
        }

        public SymmetricSecurityKey GetKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}