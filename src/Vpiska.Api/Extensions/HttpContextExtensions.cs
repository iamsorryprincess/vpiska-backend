using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserId(this HttpContext httpContext)
        {
            var userId = httpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");

            if (userId == null)
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            if (!Guid.TryParse(userId.Value, out _))
            {
                throw new InvalidOperationException("Can't resolve userId from token");
            }

            return userId.Value;
        }
    }
}