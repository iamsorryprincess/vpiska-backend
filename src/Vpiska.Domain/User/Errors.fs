namespace Vpiska.Domain.User

type ValidationError =
    | IdIsEmpty
    | NameIsEmpty
    | PhoneIsEmpty
    | PhoneRegexInvalid
    | PasswordIsEmpty
    | PasswordLengthInvalid
    | ConfirmPasswordInvalid
    | CodeIsEmpty
    | CodeLengthInvalid
    
type DomainError =
    | PhoneAlreadyUse
    | NameAlreadyUse
    | UserNotFound
    | UserByPhoneNotFound
    | InvalidPassword
    | InvalidCode
    
type AppError =
    | Validation of ValidationError
    | Domain of DomainError
    | InternalError
    static member create (error: ValidationError) = AppError.Validation error
    static member create (error: DomainError) = AppError.Domain error
    
module Errors =
    
    let mapValidationError error =
        match error with
        | IdIsEmpty -> "IdIsEmpty"
        | NameIsEmpty -> "NameIsEmpty"
        | PhoneIsEmpty -> "PhoneIsEmpty"
        | PhoneRegexInvalid -> "PhoneRegexInvalid"
        | PasswordIsEmpty -> "PasswordIsEmpty"
        | PasswordLengthInvalid -> "PasswordLengthInvalid"
        | ConfirmPasswordInvalid -> "ConfirmPasswordInvalid"
        | CodeIsEmpty -> "CodeIsEmpty"
        | CodeLengthInvalid -> "CodeLengthInvalid"
        
    let mapDomainError error =
        match error with
        | PhoneAlreadyUse -> "PhoneAlreadyUse"
        | NameAlreadyUse -> "NameAlreadyUse"
        | UserNotFound -> "UserNotFound"
        | UserByPhoneNotFound -> "UserByPhoneNotFound"
        | InvalidPassword -> "InvalidPassword"
        | InvalidCode -> "InvalidCode"
        
    let mapAppError error =
        match error with
        | Validation err -> mapValidationError err
        | Domain err -> mapDomainError err
        | InternalError -> "InternalError"
