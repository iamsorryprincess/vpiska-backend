using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Constants;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Requests;

namespace Vpiska.Domain.EventAggregate.RequestHandlers
{
    public sealed class ChatMessageHandler : RequestHandlerBase<ChatMessageRequest>
    {
        private readonly ICheckEventRepository _eventRepository;
        private readonly IChatMessageRepository _messageRepository;

        public ChatMessageHandler(IChatMessageRepository messageRepository, ICheckEventRepository eventRepository)
        {
            _messageRepository = messageRepository;
            _eventRepository = eventRepository;
        }
        
        public override async Task<DomainResponse> Handle(ChatMessageRequest request, CancellationToken cancellationToken)
        {
            var isEventNotExist = !await _eventRepository.IsEventExist(request.EventId);

            if (isEventNotExist)
            {
                return Error(DomainErrorConstants.EventNotFound);
            }

            var isFail = !await _messageRepository.AddChatMessage(request.EventId, request.UserId, request.Message);

            if (isFail)
            {
                return Error(DomainErrorConstants.UserNotFound);
            }

            //await _eventBus.Publish(request);
            return Success();
        }
    }
}