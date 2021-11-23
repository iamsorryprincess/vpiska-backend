module internal Vpiska.Domain.Event.EventValidation

open Vpiska.Domain.Validation

let private createEventRules =
    [|
        ValidationError.NameIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Name)
        ValidationError.CoordinatesIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Coordinates)
        ValidationError.AddressIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Address)
        ValidationError.AreaIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Area)
    |]
    
let private getEventsRules =
    [|
        ValidationError.AreaIsEmpty |> createRule<GetEventsArgs, ValidationError> (fun args -> isNotEmpty args.Area)
    |]
    
let private getEventRules =
    [|
        ValidationError.EventIdIsEmpty |> createRule<GetEventArgs, ValidationError> (fun args -> isNotEmpty args.EventId)
    |]
    
let private closeEventRules =
    [|
        ValidationError.EventIdIsEmpty |> createRule<CloseEventArgs, ValidationError> (fun args -> isNotEmpty args.EventId)
    |]
    
let private addMediaRules =
    [|
        ValidationError.EventIdIsEmpty |> createRule<AddMediaArgs, ValidationError> (fun args -> isNotEmpty args.EventId)
        ValidationError.MediaContentIsEmpty |> createRule<AddMediaArgs, ValidationError> (fun args -> isNotNull args.MediaData)
    |]
    
let private removeMediaRules =
    [|
        ValidationError.EventIdIsEmpty |> createRule<RemoveMediaArgs, ValidationError> (fun args -> isNotEmpty args.EventId)
        ValidationError.MediaLinkIsEmpty |> createRule<RemoveMediaArgs, ValidationError> (fun args -> isNotEmpty args.MediaLink)
    |]
    
let validateCreateEventArgs = validate createEventRules

let validateGetEventsArgs = validate getEventsRules

let validateGetEventArgs = validate getEventRules

let validateCloseEventArgs = validate closeEventRules

let validateAddMediaArgs = validate addMediaRules

let validateRemoveMediaArgs = validate removeMediaRules
