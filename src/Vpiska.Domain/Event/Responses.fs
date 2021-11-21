namespace Vpiska.Domain.Event

type AddMediaResponseArgs = { ImageId: string }

type Response =
    | EventCreated of Event
    | EventClosed
    | SubscriptionCreated
    | SubscriptionRemoved
    | UserLoggedInChat
    | UserLoggedOutFromChat
    | ChatMessageSent
    | MediaAdded of AddMediaResponseArgs
    | MediaRemoved
