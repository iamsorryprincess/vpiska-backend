using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Commands.RemoveMediaCommand;
using Vpiska.Domain.Media.Commands.UploadMediaCommand;
using Vpiska.Domain.Media.Exceptions;
using Vpiska.Domain.Media.Models;

namespace Vpiska.Infrastructure.FileStorage
{
    public sealed class FileStorage : IFileStorage
    {
        private readonly IServiceProvider _serviceProvider;

        public FileStorage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> SaveFileAsync(string filename, string contentType, Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, cancellationToken);
            await fileStream.DisposeAsync();
            var command = new UploadMediaCommand()
            {
                Name = filename,
                ContentType = contentType,
                Body = buffer
            };
            var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<UploadMediaCommand, MetadataViewModel>>();
            await commandHandler.HandleAsync(command, cancellationToken);
            return filename;
        }

        public async Task<bool> DeleteFileAsync(string filename, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new RemoveMediaCommand() { Name = filename };
                var commandHandler = _serviceProvider.GetRequiredService<ICommandHandler<RemoveMediaCommand>>();
                await commandHandler.HandleAsync(command, cancellationToken);
                return true;
            }
            catch (MediaNotFoundException)
            {
                return false;
            }
        }
    }
}