using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vpiska.Api.Filters
{
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var result = context.ModelState.ToDictionary(x => x.Key.ToLower(),
                    x => x.Value.Errors.Skip(1).Aggregate(x.Value.Errors.First().ErrorMessage,
                        (acc, item) => $"{acc}, {item.ErrorMessage}"));
                context.Result = new BadRequestObjectResult(result);
                return;
            }

            await next();
        }
    }
}