using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.EventAggregate.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class AddMediaHandler : RequestHandlerBase<AddMediaRequest, MediaResponse>
    {
        private readonly ICheckEventRepository _checkRepository;
        private readonly IAddMediaRepository _mediaRepository;
        private readonly IFirebaseStorage _firebaseStorage;

        public AddMediaHandler(ICheckEventRepository checkRepository,
            IAddMediaRepository mediaRepository,
            IFirebaseStorage firebaseStorage)
        {
            _checkRepository = checkRepository;
            _mediaRepository = mediaRepository;
            _firebaseStorage = firebaseStorage;
        }
        
        public override async Task<DomainResponse<MediaResponse>> Handle(AddMediaRequest request, CancellationToken cancellationToken)
        {
            var isEventNotExist = !await _checkRepository.IsEventExist(request.EventId);

            if (isEventNotExist)
            {
                return Error(DomainErrorConstants.EventNotFound);
            }

            var isUserNotOwner = !await _checkRepository.CheckOwnership(request.EventId, request.OwnerId);

            if (isUserNotOwner)
            {
                return Error(DomainErrorConstants.UserNotOwner);
            }

            await using var stream = new MemoryStream(request.MediaData);
            var link = await _firebaseStorage.UploadFile(Guid.NewGuid().ToString(), request.ContentType, stream);
            var isSuccess = await _mediaRepository.AddMedia(request.EventId, link);
            return isSuccess
                ? Success(new MediaResponse() { MediaLink = link })
                : Error(DomainErrorConstants.MediaAlreadyAdded);
        }
    }
}