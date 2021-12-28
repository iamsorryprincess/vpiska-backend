using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Vpiska.Jwt
{
    public static class Entry
    {
        public static void AddJwt(this IServiceCollection services, IConfigurationSection jwtSection)
        {
            Jwt.Audience = jwtSection["Audience"];
            Jwt.Issuer = jwtSection["Issuer"];
            Jwt.Key = jwtSection["Key"];
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.AddJwtOptions());
        }

        public static void AddJwtWebSocket(this IServiceCollection services, IConfigurationSection jwtSection,
            string tokenQueryParam,
            string webSocketUrl)
        {
            Jwt.Audience = jwtSection["Audience"];
            Jwt.Issuer = jwtSection["Issuer"];
            Jwt.Key = jwtSection["Key"];
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
                ValidIssuer = Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = Jwt.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = Jwt.GetKey(Jwt.Key),
                ValidateIssuerSigningKey = true
            };
        }
    }
}