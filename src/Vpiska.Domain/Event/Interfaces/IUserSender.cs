using System;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IUserSender
    {
        Task SendEventUpdatedInfo(Guid[] connections, EventUpdatedInfo eventUpdatedInfo);

        Task SendEventCreated(Guid[] connections, EventShortResponse eventShortResponse);

        Task SendEventClosed(Guid[] connections, string eventId);
    }
}