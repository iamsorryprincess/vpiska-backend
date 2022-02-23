using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Queries.GetEventsQuery
{
    internal sealed class GetEventsHandler : IQueryHandler<GetEventsQuery, List<EventShortResponse>>
    {
        private readonly IValidator<GetEventsQuery> _validator;
        private readonly IEventStorage _eventStorage;

        public GetEventsHandler(IValidator<GetEventsQuery> validator, IEventStorage eventStorage)
        {
            _validator = validator;
            _eventStorage = eventStorage;
        }

        public async Task<List<EventShortResponse>> HandleAsync(GetEventsQuery query, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(query, cancellationToken: cancellationToken);
            var halfHorizontalRange = query.HorizontalRange.Value / 2;
            var halfVerticalRange = query.VerticalRange.Value / 2;
            var xLeft = query.Coordinates.X.Value - halfHorizontalRange;
            var xRight = query.Coordinates.X.Value + halfHorizontalRange;
            var yLeft = query.Coordinates.Y.Value - halfVerticalRange;
            var yRight = query.Coordinates.Y.Value + halfVerticalRange;
            var result = await _eventStorage.GetDataByRange(xLeft, xRight, yLeft, yRight);
            return result;
        }
    }
}