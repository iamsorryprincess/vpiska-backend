using System.Threading.Tasks;
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
                .AddJwtBearer(options => options.AddJwtOptions());
            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IPasswordSecurityService, PasswordSecurityService>();
        }

        public static void AddJwtForWebSocket(this IServiceCollection services, string tokenQueryParam,
            string webSocketUrl)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.AddJwtOptions();
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query[tokenQueryParam];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(webSocketUrl))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();
        }

        private static void AddJwtOptions(this JwtBearerOptions options)
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
        }
    }
}