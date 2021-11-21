namespace Vpiska.Domain.Event

type ValidationError =
    | AreaIsEmpty
    | EventIdIsEmpty
    | CoordinatesIsEmpty
    | NameIsEmpty
    | AddressIsEmpty
    | MediaContentIsEmpty
    | MediaLinkIsEmpty
    
type DomainError =
    | AreaNotFound
    | EventNotFound
    | UserNotFound
    | UserAlreadyExists
    | OwnerAlreadyHasEvent
    | UserNotOwner
    | AreaAlreadyHasEvent
    | MediaAlreadyAdded
    | MediaNotFound
    | SubscriptionAlreadyExist
    | SubscriptionNotFound
    
type AppError =
    | Validation of ValidationError
    | Domain of DomainError
    | InternalError
    static member create (error: ValidationError) = AppError.Validation error
    static member create (error: DomainError) = AppError.Domain error
    
module Errors =
    
    let mapValidationError error =
        match error with
        | AreaIsEmpty -> "AreaIsEmpty"
        | EventIdIsEmpty -> "EventIdIsEmpty"
        | CoordinatesIsEmpty -> "CoordinatesIsEmpty"
        | NameIsEmpty -> "NameIsEmpty"
        | AddressIsEmpty -> "AddressIsEmpty"
        | MediaContentIsEmpty -> "MediaContentIsEmpty"
        | MediaLinkIsEmpty -> "MediaLinkIsEmpty"
        
    let mapDomainError error =
        match error with
        | AreaNotFound -> "AreaNotFound"
        | EventNotFound -> "EventNotFound"
        | UserNotFound -> "UserNotFound"
        | UserAlreadyExists -> "UserAlreadyExists"
        | OwnerAlreadyHasEvent -> "OwnerAlreadyHasEvent"
        | UserNotOwner -> "UserNotOwner"
        | AreaAlreadyHasEvent -> "AreaAlreadyHasEvent"
        | MediaAlreadyAdded -> "MediaAlreadyAdded"
        | MediaNotFound -> "MediaNotFound"
        | SubscriptionAlreadyExist -> "SubscriptionAlreadyExist"
        | SubscriptionNotFound -> "SubscriptionNotFound"
        
    let mapAppError error =
        match error with
        | Validation validationError -> mapValidationError validationError
        | Domain domainError -> mapDomainError domainError
        | InternalError -> "InternalError"
