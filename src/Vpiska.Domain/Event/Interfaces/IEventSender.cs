using System;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventSender
    {
        Task SendUsersCountUpdate(Guid[] connections, int usersCount);

        Task SendChatMessageToConnections(Guid[] connectionIds, ChatMessage chatMessage);

        Task SendCloseStatus(Guid connectionId);

        Task NotifyMediaAdded(Guid[] connections, MediaInfo mediaInfo);

        Task NotifyMediaRemoved(Guid[] connections, string mediaId);
    }
}