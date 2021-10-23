using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Vpiska.Domain.Base
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, DomainResponse<TResponse>>
        where TRequest : IRequest<DomainResponse<TResponse>>
        where TResponse : class
    {
        public abstract Task<DomainResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);

        protected DomainResponse<TResponse> Error(params string[] errorCodes) =>
            DomainResponse<TResponse>.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());

        protected DomainResponse<TResponse> Success(TResponse response) =>
            DomainResponse<TResponse>.CreateSuccess(response);
    }

    public abstract class RequestHandlerBase<TRequest> : IRequestHandler<TRequest, DomainResponse>
        where TRequest : IRequest<DomainResponse>
    {
        public abstract Task<DomainResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected DomainResponse Error(params string[] errorCodes) =>
            DomainResponse.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());

        protected DomainResponse Success() => DomainResponse.CreateSuccess();
    }
}