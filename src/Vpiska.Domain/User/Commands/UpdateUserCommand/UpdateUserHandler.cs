using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;

namespace Vpiska.Domain.User.Commands.UpdateUserCommand
{
    internal sealed class UpdateUserHandler : ValidationCommandHandler<UpdateUserCommand, ImageIdResponse>
    {
        private readonly IUserRepository _repository;
        private readonly IFileStorage _fileStorage;
        
        public UpdateUserHandler(IValidator<UpdateUserCommand> validator,
            IUserRepository repository,
            IFileStorage fileStorage) : base(validator)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        protected override async Task<ImageIdResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Name) && string.IsNullOrWhiteSpace(command.Phone) &&
                command.ImageStream == null)
            {
                return new ImageIdResponse();
            }

            var user = await _repository.GetByFieldAsync("_id", command.Id, cancellationToken);

            if (user == null)
            {
                throw new UserNotFoundException(Constants.UserNotFound);
            }

            var checkResult = await _repository.CheckPhoneAndNameWithEmptyParams(command.Phone, command.Name, cancellationToken);
            
            switch (checkResult.IsNameExist)
            {
                case true when !checkResult.IsPhoneExist:
                    throw new UserNameAlreadyExistException();
                case false when checkResult.IsPhoneExist:
                    throw new UserPhoneAlreadyExistException();
                case true:
                    throw new UserPhoneAndNameAlreadyExistException();
                default:
                {
                    var imageId = command.ImageStream == null
                        ? user.ImageId
                        : string.IsNullOrWhiteSpace(user.ImageId)
                            ? await _fileStorage.SaveFileAsync(Guid.NewGuid().ToString(), command.ContentType, command.ImageStream,
                                cancellationToken)
                            : await _fileStorage.SaveFileAsync(user.ImageId, command.ContentType, command.ImageStream,
                                cancellationToken);

                    await _repository.UpdateUser(command.Id, command.Name, command.Phone, imageId, cancellationToken);
                    return new ImageIdResponse() { ImageId = imageId };
                }
            }
        }
    }
}