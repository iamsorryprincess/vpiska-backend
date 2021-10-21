using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Api.Filters
{
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(error => ErrorResponse.Create(error.ErrorMessage))
                    .ToArray();
                context.Result = new ObjectResult(DomainResponse.CreateError(errors)) { StatusCode = 200 };
                return;
            }

            await next();
        }
    }
}