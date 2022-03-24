using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Interfaces;
using Vpiska.Domain.Media.Models;

namespace Vpiska.Domain.Media.Queries.PageQuery
{
    internal sealed class PageQueryHandler : IQueryHandler<PageQuery, PagedResponse>
    {
        private readonly IMediaRepository _repository;

        public PageQueryHandler(IMediaRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResponse> HandleAsync(PageQuery query, CancellationToken cancellationToken = default)
        {
            if (query.Page <= 0)
            {
                query.Page = 1;
            }

            if (query.Size <= 0)
            {
                query.Size = 20;
            }

            return _repository.GetPagedFilesMetadata(query.Page, query.Size, cancellationToken);
        }
    }
}