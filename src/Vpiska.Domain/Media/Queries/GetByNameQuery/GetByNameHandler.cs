using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Exceptions;
using Vpiska.Domain.Media.Interfaces;

namespace Vpiska.Domain.Media.Queries.GetByNameQuery
{
    internal sealed class GetByNameHandler : IQueryHandler<GetByNameQuery, Media>
    {
        private readonly ICache<Media> _cache;
        private readonly IMediaRepository _repository;

        public GetByNameHandler(ICache<Media> cache, IMediaRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public async Task<Media> HandleAsync(GetByNameQuery query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.Name))
            {
                throw new NameIsEmptyException();
            }
            
            var media = await _cache.Get(query.Name);

            if (media == null)
            {
                media = await _repository.GetByFieldAsync("name", query.Name, cancellationToken);

                if (media == null)
                {
                    throw new MediaNotFoundException();
                }

                await _cache.Set(query.Name, media);
            }

            if (!File.Exists($"{Constants.Path}/{media.Name}.{media.Extension}"))
            {
                await _cache.Remove(query.Name);
                await _repository.RemoveByFieldAsync("name", media.Name, cancellationToken);
                throw new MediaNotFoundException();
            }

            return media;
        }
    }
}