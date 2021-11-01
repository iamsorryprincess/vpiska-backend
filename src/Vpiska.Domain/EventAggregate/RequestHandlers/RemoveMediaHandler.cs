using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class RemoveMediaHandler : RequestHandlerBase<RemoveMediaRequest>
    {
        private readonly ICheckEventRepository _checkEventRepository;
        private readonly IRemoveMediaRepository _mediaRepository;
        private readonly IFirebaseStorage _firebaseStorage;

        public RemoveMediaHandler(ICheckEventRepository checkEventRepository,
            IRemoveMediaRepository mediaRepository,
            IFirebaseStorage firebaseStorage)
        {
            _checkEventRepository = checkEventRepository;
            _mediaRepository = mediaRepository;
            _firebaseStorage = firebaseStorage;
        }
        
        public override async Task<DomainResponse> Handle(RemoveMediaRequest request, CancellationToken cancellationToken)
        {
            var isEventNotExist = !await _checkEventRepository.IsEventExist(request.EventId);

            if (isEventNotExist)
            {
                return Error(DomainErrorConstants.EventNotFound);
            }

            var isNotOwner = !await _checkEventRepository.CheckOwnership(request.EventId, request.OwnerId);

            if (isNotOwner)
            {
                return Error(DomainErrorConstants.UserNotOwner);
            }

            var isMediaNotFound = !await _mediaRepository.RemoveMedia(request.EventId, request.MediaLink);

            if (isMediaNotFound)
            {
                return Error(DomainErrorConstants.MediaNotFound);
            }

            await _firebaseStorage.DeleteFile(request.MediaLink);
            return Success();
        }
    }
}