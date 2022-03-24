using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Interfaces;
using Vpiska.Domain.Media.Models;

namespace Vpiska.Domain.Media.Commands.UploadMediaCommand
{
    internal sealed class UploadMediaHandler : ICommandHandler<UploadMediaCommand, MetadataViewModel>
    {
        private readonly IMediaRepository _repository;
        private readonly ICache<Media> _cache;

        public UploadMediaHandler(IMediaRepository repository, ICache<Media> cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<MetadataViewModel> HandleAsync(UploadMediaCommand command, CancellationToken cancellationToken = default)
        {
            var existingModel = await _repository.GetByFieldAsync("name", command.Name, cancellationToken);
            var model = existingModel != null
                ? command.ToModel(existingModel.Id, command.ContentType.GetExtension())
                : command.ToModel(Guid.NewGuid().ToString(), command.ContentType.GetExtension());
            model.LastModifiedDate = DateTime.Now;

            if (existingModel != null)
            {
                await _cache.Remove(command.Name);
                await _repository.UpdateAsync(model, cancellationToken);
            }
            else
            {
                await _repository.InsertAsync(model, cancellationToken);
            }
            
            await _cache.Set(model.Id, model);
            await File.WriteAllBytesAsync($"{Constants.Path}/{command.Name}.{model.Extension}",
                command.Body,
                cancellationToken);

            return new MetadataViewModel()
            {
                Name = model.Name,
                Size = model.Size.MapToSize(),
                Type = model.ContentType,
                LastModified = model.LastModifiedDate.ToString("dd.MM.yyyy")
            };
        }
    }
}