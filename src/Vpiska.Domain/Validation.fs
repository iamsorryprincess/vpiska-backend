module Vpiska.Domain.Validation

open System
open System.Text.RegularExpressions
open Vpiska.Domain.Errors
open Vpiska.Domain.Commands

type ValidationCondition<'a> = 'a -> bool
type ValidationRule<'a> =
    { Condition: ValidationCondition<'a>
      Error: ValidationError }
    
let private createRule<'a> (condition: ValidationCondition<'a>) (error: ValidationError) =
    { Condition = condition
      Error = error }

let private isNotEmpty input = String.IsNullOrWhiteSpace(input) |> not
let private isRegex pattern input = Regex.IsMatch(input, pattern)
let private isLength (input: string) length = input.Length = length
let private isEqual input eqValue = input = eqValue
    
let private validate<'a> (rules: ValidationRule<'a>[]) (request: 'a) =
    let errors = rules |> Array.filter (fun rule -> rule.Condition request |> not) |> Array.map (fun rule -> rule.Error)
    if errors.Length = 0
    then Ok request
    else Error errors
    
let private phoneRegex = @"^\d{10}\b$"
let private passwordLength = 6
    
let private createUserRules =
    [|
        ValidationError.PhoneIsEmpty |> createRule<CreateUserArgs> (fun args -> isNotEmpty args.Phone)
        ValidationError.PhoneRegexInvalid |> createRule<CreateUserArgs> (fun args -> isRegex phoneRegex args.Phone)
        ValidationError.NameIsEmpty |> createRule<CreateUserArgs> (fun args -> isNotEmpty args.Name)
        ValidationError.PasswordIsEmpty |> createRule<CreateUserArgs> (fun args -> isNotEmpty args.Password)
        ValidationError.PasswordLengthInvalid |> createRule<CreateUserArgs> (fun args -> isLength args.Password passwordLength)
        ValidationError.ConfirmPasswordInvalid |> createRule<CreateUserArgs> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let private loginUserRules =
    [|
        ValidationError.PhoneIsEmpty |> createRule<LoginUserArgs> (fun args -> isNotEmpty args.Phone)
        ValidationError.PhoneRegexInvalid |> createRule<LoginUserArgs> (fun args -> isRegex phoneRegex args.Phone)
        ValidationError.PasswordIsEmpty |> createRule<LoginUserArgs> (fun args -> isNotEmpty args.Password)
        ValidationError.PasswordLengthInvalid |> createRule<LoginUserArgs> (fun args -> isLength args.Password passwordLength)
    |]
    
let private codeRules =
    [|
        ValidationError.PhoneIsEmpty |> createRule<CodeArgs> (fun args -> isNotEmpty args.Phone)
        ValidationError.PhoneRegexInvalid |> createRule<CodeArgs> (fun args -> isRegex phoneRegex args.Phone)
    |]
    
let private checkCodeRules =
    [|
        ValidationError.PhoneIsEmpty |> createRule<CheckCodeArgs> (fun args -> isNotEmpty args.Phone)
        ValidationError.PhoneRegexInvalid |> createRule<CheckCodeArgs> (fun args -> isRegex phoneRegex args.Phone)
        ValidationError.CodeIsEmpty |> createRule<CheckCodeArgs> (fun args -> args.Code.HasValue)
        ValidationError.CodeLengthInvalid |> createRule<CheckCodeArgs> (fun args -> args.Code.HasValue && args.Code.Value < 1000000 && args.Code.Value >= 100000)
    |]
    
let private changePasswordRules =
    [|
        ValidationError.IdIsEmpty |> createRule<ChangePasswordArgs> (fun args -> isNotEmpty args.Id)
        ValidationError.PasswordIsEmpty |> createRule<ChangePasswordArgs> (fun args -> isNotEmpty args.Password)
        ValidationError.PasswordLengthInvalid |> createRule<ChangePasswordArgs> (fun args -> isLength args.Password passwordLength)
        ValidationError.ConfirmPasswordInvalid |> createRule<ChangePasswordArgs> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let validateCreateUserArgs (args: CreateUserArgs) = validate createUserRules args
let validateLoginUserArgs (args: LoginUserArgs) = validate loginUserRules args
let validateCodeArgs (args: CodeArgs) = validate codeRules args
let validateCheckCodeArgs (args: CheckCodeArgs) = validate checkCodeRules args
let validateChangePassword (args: ChangePasswordArgs) = validate changePasswordRules args
