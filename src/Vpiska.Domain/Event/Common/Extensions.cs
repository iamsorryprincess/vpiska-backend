using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Common
{
    internal static class Extensions
    {
        public static async Task<Event> GetEvent(this IEventStorage storage, IEventRepository repository, string eventId,
            CancellationToken cancellationToken = default)
        {
            var model = await storage.GetData(eventId);

            if (model != null)
            {
                return model;
            }

            model = await repository.GetByFieldAsync("_id", eventId, cancellationToken);

            if (model == null)
            {
                return null;
            }

            await storage.SetData(model);
            return model;
        }
    }
}