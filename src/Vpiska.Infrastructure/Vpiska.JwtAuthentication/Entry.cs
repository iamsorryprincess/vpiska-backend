using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Vpiska.Domain.Interfaces;

namespace Vpiska.JwtAuthentication
{
    public static class Entry
    {
        public static void AddJwt(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = JwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = JwtOptions.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });
            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IPasswordSecurityService, PasswordSecurityService>();
        }
    }
}