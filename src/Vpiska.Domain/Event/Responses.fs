namespace Vpiska.Domain.Event

type Response =
    | EventCreated of Event
    | SubscriptionCreated
    | SubscriptionRemoved
