module internal Vpiska.Domain.Event.EventValidation

open Vpiska.Domain.Validation

let private createEventRules =
    [|
        ValidationError.NameIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Name)
        ValidationError.CoordinatesIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Coordinates)
        ValidationError.AddressIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Address)
        ValidationError.AreaIsEmpty |> createRule<CreateEventArgs, ValidationError> (fun args -> isNotEmpty args.Area)
    |]
    
let validateCreateEventArgs = validate createEventRules
