using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Queries.GetByIdQuery
{
    internal sealed class GetByIdHandler : ValidationQueryHandler<GetByIdQuery, EventResponse>
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;

        public GetByIdHandler(IValidator<GetByIdQuery> validator,
            ICache<Event> cache,
            IEventRepository repository) : base(validator)
        {
            _cache = cache;
            _repository = repository;
        }

        protected override async Task<EventResponse> Handle(GetByIdQuery query, CancellationToken cancellationToken)
        {
            var model = await _cache.GetData(query.EventId);

            if (model != null)
            {
                return EventResponse.FromModel(model);
            }

            model = await _repository.GetByFieldAsync("_id", query.EventId, cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }

            await _cache.SetData(model);
            return EventResponse.FromModel(model);
        }
    }
}