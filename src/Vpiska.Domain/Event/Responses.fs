namespace Vpiska.Domain.Event

type ShortEventResponse =
    { Id: string
      Name: string
      Coordinates: string
      UsersCount: int }

type Response =
    | Event of Event
    | Events of ShortEventResponse[]
