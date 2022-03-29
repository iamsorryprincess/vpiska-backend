using System;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IUserSender
    {
        Task SendEventUpdatedInfo(Guid[] connections, EventUpdatedInfo eventUpdatedInfo);

        Task SendEventCreated(Guid[] connections, EventCreatedInfo eventCreatedInfo);

        Task SendEventClosed(Guid[] connections, string eventId);
    }
}