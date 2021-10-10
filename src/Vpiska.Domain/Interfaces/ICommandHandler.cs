using System.Threading.Tasks;
using Vpiska.Domain.Responses;

namespace Vpiska.Domain.Interfaces
{
    public interface ICommandHandler<in TRequest>
    {
        Task<DomainResponse> Handle(TRequest request);
    }
    
    public interface ICommandHandler<in TRequest, TResponse>
    {
        Task<DomainResponse<TResponse>> Handle(TRequest request);
    }
}