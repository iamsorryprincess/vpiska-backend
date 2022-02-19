using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Queries.GetByIdQuery
{
    internal sealed class GetByIdHandler : ValidationQueryHandler<GetByIdQuery, EventResponse>
    {
        private readonly IEventStorage _eventState;
        private readonly IEventRepository _repository;

        public GetByIdHandler(IValidator<GetByIdQuery> validator,
            IEventStorage eventState,
            IEventRepository repository) : base(validator)
        {
            _eventState = eventState;
            _repository = repository;
        }

        protected override async Task<EventResponse> Handle(GetByIdQuery query, CancellationToken cancellationToken)
        {
            var model = await _eventState.GetData(query.EventId);

            if (model != null)
            {
                return EventResponse.FromModel(model);
            }

            model = await _repository.GetByFieldAsync("_id", query.EventId, cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }

            await _eventState.SetData(model);
            return EventResponse.FromModel(model);
        }
    }
}