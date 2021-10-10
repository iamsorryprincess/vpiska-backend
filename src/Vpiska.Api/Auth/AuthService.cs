using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Api.Auth
{
    public sealed class AuthService : IAuthService
    {
        private const int LifetimeDays = 3;
        
        public string GetEncodedJwt(string userId, string username, string imageUrl)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", userId),
                new Claim("Name", username)
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                claims.Add(new Claim("ImageUrl", imageUrl));
            }

            var now = DateTime.UtcNow;
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.Issuer,
                audience: JwtOptions.Audience,
                notBefore: now,
                claims: claimsIdentity.Claims,
                expires: now.Add(TimeSpan.FromDays(LifetimeDays)),
                signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public string HashPassword(string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

        public bool CheckPassword(string hash, string password) =>
            hash == Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }
}