using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.UserAggregate.Constants;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public sealed class UpdateUserHandler : RequestHandlerBase<UpdateUserRequest, DomainResponse>
    {
        private readonly IGetByIdRepository _idRepository;
        private readonly IUpdateUserRepository _updateRepository;
        private readonly IFirebaseStorage _firebaseStorage;

        public UpdateUserHandler(IGetByIdRepository idRepository,
            IUpdateUserRepository updateRepository,
            IFirebaseStorage firebaseStorage)
        {
            _idRepository = idRepository;
            _updateRepository = updateRepository;
            _firebaseStorage = firebaseStorage;
        }
        
        public override async Task<DomainResponse> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _idRepository.GetById(request.Id);

            if (user == null)
            {
                return Error(DomainErrorConstants.UserNotFound);
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var isPhoneExist = await _updateRepository.IsPhoneExist(request.Phone);

                if (isPhoneExist)
                {
                    return Error(DomainErrorConstants.PhoneAlreadyUse);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var isNameExist = await _updateRepository.IsNameExist(request.Phone);
                
                if (isNameExist)
                {
                    return Error(DomainErrorConstants.NameAlreadyUse);
                }
            }

            if (request.ImageData != null)
            {
                await using var stream = new MemoryStream(request.ImageData);
                var imageId = string.IsNullOrWhiteSpace(user.ImageId)
                    ? await _firebaseStorage.UploadFile(Guid.NewGuid().ToString(), request.ContentType, stream)
                    : await _firebaseStorage.UploadFile(user.ImageId, request.ContentType, stream);
                await _updateRepository.Update(request.Id, request.Name, request.Phone, imageId);
                return Success();
            }

            await _updateRepository.Update(request.Id, request.Name, request.Phone, user.ImageId);
            return Success();
        }
    }
}