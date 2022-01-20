using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Infrastructure.Identity
{
    internal sealed class JwtTokenService : IIdentityService
    {
        private readonly IdentitySettings _settings;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtTokenService(IdentitySettings settings)
        {
            _settings = settings;
        }
        
        public string GetAccessToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim("Id", user.Id),
                new Claim("Name", user.Name)
            };

            if (!string.IsNullOrWhiteSpace(user.ImageId))
                claims.Add(new Claim("ImageId", user.ImageId));

            var now = DateTime.Now;
            var identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            var jwt = new JwtSecurityToken(issuer: _settings.Issuer, audience: _settings.Audience, notBefore: now, claims: identity.Claims,
                expires: now.Add(TimeSpan.FromDays(_settings.LifetimeDays)),
                signingCredentials: new SigningCredentials(_settings.GetKey(), SecurityAlgorithms.HmacSha256));
            return _tokenHandler.WriteToken(jwt);
        }
    }
}