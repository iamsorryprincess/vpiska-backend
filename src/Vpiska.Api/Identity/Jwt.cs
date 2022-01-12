using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Vpiska.Api.Identity
{
    public static class Jwt
    {
        internal static string Key;
        internal static string Issuer;
        internal static string Audience;
        
        private const int LifetimeDays = 3;

        private static readonly JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();

        public static string EncodeJwt(string userId, string userName, string imageId)
        {
            var claims = new List<Claim>()
            {
                new Claim("Id", userId),
                new Claim("Name", userName)
            };

            if (!string.IsNullOrWhiteSpace(imageId))
                claims.Add(new Claim("ImageId", imageId));

            var now = DateTime.Now;
            var identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            var key = GetKey(Key);

            var jwt = new JwtSecurityToken(issuer: Issuer, audience: Audience, notBefore: now, claims: identity.Claims,
                expires: now.Add(TimeSpan.FromDays(LifetimeDays)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return TokenHandler.WriteToken(jwt);
        }

        internal static SymmetricSecurityKey GetKey(string key) =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}