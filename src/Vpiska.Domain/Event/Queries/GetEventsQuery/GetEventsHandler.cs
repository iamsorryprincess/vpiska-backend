using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Queries.GetEventsQuery
{
    internal sealed class GetEventsHandler : ValidationQueryHandler<GetEventsQuery, List<EventShortResponse>>
    {
        private readonly IEventStorage _eventState;

        public GetEventsHandler(IValidator<GetEventsQuery> validator, IEventStorage eventState) : base(validator)
        {
            _eventState = eventState;
        }

        protected override Task<List<EventShortResponse>> Handle(GetEventsQuery query, CancellationToken cancellationToken)
        {
            var halfHorizontalRange = query.HorizontalRange.Value / 2;
            var halfVerticalRange = query.VerticalRange.Value / 2;
            var xLeft = query.Coordinates.X.Value - halfHorizontalRange;
            var xRight = query.Coordinates.X.Value + halfHorizontalRange;
            var yLeft = query.Coordinates.Y.Value - halfVerticalRange;
            var yRight = query.Coordinates.Y.Value + halfVerticalRange;
            return _eventState.GetDataByRange(xLeft, xRight, yLeft, yRight);
        }
    }
}