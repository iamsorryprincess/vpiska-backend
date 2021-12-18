namespace Vpiska.Domain.Event

type EventId = string
type UserId = string
type EventName = string

type ChatData =
    { UserId: UserId
      UserName: string
      UserImage: string
      Message: string }
    
type UserInfo =
    { Id: UserId
      Name: string
      ImageId: string }
    
type Event =
    { Id: EventId
      OwnerId: UserId
      Name: EventName
      Coordinates: string
      Address: string
      MediaLinks: string[]
      ChatData: ChatData[]
      Users: UserInfo[] }
