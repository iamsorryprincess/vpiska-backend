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
    internal sealed class GetByIdHandler : IQueryHandler<GetByIdQuery, EventResponse>
    {
        private readonly IValidator<GetByIdQuery> _validator;
        private readonly IEventStorage _eventStorage;
        private readonly IEventRepository _repository;

        public GetByIdHandler(IValidator<GetByIdQuery> validator,
            IEventStorage eventStorage,
            IEventRepository repository)
        {
            _validator = validator;
            _eventStorage = eventStorage;
            _repository = repository;
        }

        public async Task<EventResponse> HandleAsync(GetByIdQuery query, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(query, cancellationToken: cancellationToken);
            var model = await _eventStorage.GetData(query.EventId);

            if (model != null)
            {
                return EventResponse.FromModel(model);
            }

            model = await _repository.GetByFieldAsync("_id", query.EventId, cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }

            await _eventStorage.SetData(model);
            return EventResponse.FromModel(model);
        }
    }
}