namespace Vpiska.Domain.Event.Logic

open System.Threading.Tasks
open Vpiska.Domain.Event

type Area = string
type ImageId = string
type ContentType = string

type CheckArea = Area -> bool
type CheckOwner = Area -> UserId -> Task<bool>
type CreateEvent = Area -> Event -> Task<bool>
type CheckOwnership = EventId -> UserId -> Task<bool>
type CloseEvent = EventId -> Task<bool>

type CreateSubscription = EventId -> Task<bool>
type RemoveSubscription = EventId -> Task<bool>
type PublishEvent = EventId -> DomainEvent -> Task

type CheckEvent = EventId -> Task<bool>
type AddUser = EventId -> UserInfo -> Task<bool>
type GetUsers = EventId -> Task<UserInfo[]>
type RemoveUser = EventId -> UserId -> Task<bool>
type AddMessage = EventId -> ChatData -> Task<bool>

type UploadFile = ImageId -> byte[] -> ContentType -> Task<ImageId>
type AddMedia = EventId -> ImageId -> Task<bool>
type RemoveMedia = EventId -> ImageId -> Task<bool>
type DeleteFile = ImageId -> Task<bool>

type GetEvent = EventId -> Task<Event>
type GetEvents = Area -> Task<ShortEventResponse[]>
