using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Exceptions;
using Vpiska.Domain.Media.Interfaces;

namespace Vpiska.Domain.Media.Commands.RemoveMediaCommand
{
    internal sealed class RemoveMediaHandler : ICommandHandler<RemoveMediaCommand>
    {
        private readonly IMediaRepository _repository;
        private readonly ICache<Media> _cache;

        public RemoveMediaHandler(IMediaRepository repository, ICache<Media> cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task HandleAsync(RemoveMediaCommand command, CancellationToken cancellationToken = default)
        {
            await _cache.Remove(command.Name);
            var media = await _repository.GetByFieldAsync("name", command.Name, cancellationToken);

            if (media == null)
            {
                throw new MediaNotFoundException();
            }

            await _repository.RemoveByFieldAsync("name", command.Name, cancellationToken);
            var path = $"{Constants.Path}/{command.Name}.{media.Extension}";
            
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}