using System;
using System.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Vpiska.Api.Auth
{
    internal static class JwtOptions
    {
        private const string Key = "vpiska_secretkey!123";
        private const int LifetimeDays = 3;
        
        public const string Issuer = "VpiskaServer";
        public const string Audience = "VpiskaClient";

        public static string GetEncodedJwt(string userId, string name, string imageUrl)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", userId),
                new Claim("Name", name)
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                claims.Add(new Claim("ImageUrl", imageUrl));
            }

            var now = DateTime.UtcNow;
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var jwt = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                notBefore: now,
                claims: claimsIdentity.Claims,
                expires: now.Add(TimeSpan.FromDays(LifetimeDays)),
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
        
        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}