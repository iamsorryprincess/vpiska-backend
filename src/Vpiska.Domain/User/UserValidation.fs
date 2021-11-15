module internal Vpiska.Domain.User.UserValidation

open System
open System.Text.RegularExpressions
open Vpiska.Domain.Validation

let private isNotEmpty input = String.IsNullOrWhiteSpace(input) |> not
let private isRegex pattern input = Regex.IsMatch(input, pattern)
let private isGreaterThan (input: string) length = input.Length >= length
let private isEqual input eqValue = input = eqValue
   
let private phoneRegex = @"^\d{10}\b$"
let private passwordLength = 6

let private createUserRules =
    [|
        createWithThenRules<CreateUserArgs, ValidationError> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CreateUserArgs, ValidationError> (fun args -> isRegex phoneRegex args.Phone)
        |]
        
        ValidationError.NameIsEmpty |> createRule<CreateUserArgs, ValidationError> (fun args -> isNotEmpty args.Name)
        
        createWithThenRules<CreateUserArgs, ValidationError> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<CreateUserArgs, ValidationError> (fun args -> isGreaterThan args.Password passwordLength)
        |]
        
        ValidationError.ConfirmPasswordInvalid |> createRule<CreateUserArgs, ValidationError> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let private loginUserRules =
    [|
        createWithThenRules<LoginUserArgs, ValidationError> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<LoginUserArgs, ValidationError> (fun args -> isRegex phoneRegex args.Phone)
        |]
        createWithThenRules<LoginUserArgs, ValidationError> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<LoginUserArgs, ValidationError> (fun args -> isGreaterThan args.Password passwordLength)
        |]
    |]
    
let private codeRules =
    [|
        createWithThenRules<CodeArgs, ValidationError> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CodeArgs, ValidationError> (fun args -> isRegex phoneRegex args.Phone)
        |]
    |]
    
let private checkCodeRules =
    [|
        createWithThenRules<CheckCodeArgs, ValidationError> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CheckCodeArgs, ValidationError> (fun args -> isRegex phoneRegex args.Phone)
        |]
        createWithThenRules<CheckCodeArgs, ValidationError> ValidationError.CodeIsEmpty (fun args -> args.Code.HasValue) [|
            ValidationError.CodeLengthInvalid |> createRule<CheckCodeArgs, ValidationError> (fun args -> args.Code.HasValue && args.Code.Value < 1000000 && args.Code.Value >= 100000)
        |]
    |]
    
let private changePasswordRules =
    [|
        ValidationError.IdIsEmpty |> createRule<ChangePasswordArgs, ValidationError> (fun args -> isNotEmpty args.Id)
        createWithThenRules<ChangePasswordArgs, ValidationError> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<ChangePasswordArgs, ValidationError> (fun args -> isGreaterThan args.Password passwordLength)
        |]
        ValidationError.ConfirmPasswordInvalid |> createRule<ChangePasswordArgs, ValidationError> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let validateCreateUserArgs (args: CreateUserArgs) = validate createUserRules args

let validateLoginUserArgs (args: LoginUserArgs) = validate loginUserRules args

let validateCodeArgs (args: CodeArgs) = validate codeRules args

let validateCheckCodeArgs (args: CheckCodeArgs) = validate checkCodeRules args

let validateChangePasswordArgs (args: ChangePasswordArgs) = validate changePasswordRules args

let validateUpdateUserArgs (args: UpdateUserArgs) =
    match isNotEmpty args.Phone with
    | false -> Ok args
    | true -> if isRegex phoneRegex args.Phone then Ok args else [|ValidationError.PhoneRegexInvalid|] |> Error
