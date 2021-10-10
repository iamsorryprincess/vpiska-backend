using System;
using System.Threading.Tasks;
using Vpiska.Domain;
using Vpiska.Domain.Commands;
using Vpiska.Domain.Responses;

namespace Vpiska.Api
{
    internal static class MappingExtensions
    {
        public static async Task<Response<TResponse>> Handle<TResponse>(this CommandHandler handler, Command command) where TResponse : class
        {
            var result = await handler.Handle(command);
            return result.IsError
                ? Response<TResponse>.error<TResponse>(result.ErrorValue)
                : Response<TResponse>.success<TResponse>(result.ResultValue.MapResponse<TResponse>());
        }

        public static async Task<Response> HandleWithNoResponse(this CommandHandler handler, Command command)
        {
            var result = await handler.Handle(command);
            return result.IsError
                ? Response.error(result.ErrorValue)
                : Response.success();
        }

        private static TResponse MapResponse<TResponse>(this DomainResponse response) where TResponse : class
        {
            return response switch
            {
                DomainResponse.Login result => result.Item as TResponse,
                _ => throw new ArgumentException("Unknown mapping type for response")
            };
        }
    }
}