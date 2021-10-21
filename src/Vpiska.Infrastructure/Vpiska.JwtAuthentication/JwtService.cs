using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Vpiska.Domain.Interfaces;

namespace Vpiska.JwtAuthentication
{
    internal sealed class JwtService : IJwtService
    {
        private const int LifetimeDays = 3;
        
        public string EncodeJwt(Guid userId, string username, string imageId)
        {
            var claims = new List<Claim>
            {
                new("Id", userId.ToString()),
                new("Name", username)
            };

            if (!string.IsNullOrWhiteSpace(imageId))
            {
                claims.Add(new Claim("ImageId", imageId));
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
    }
}