module Vpiska.Domain.Validation

open System
open System.Text.RegularExpressions
open Vpiska.Domain.Errors
open Vpiska.Domain.Commands

type ValidationCondition<'a> = 'a -> bool

type ValidationRule<'a> =
    { Condition: ValidationCondition<'a>
      Error: ValidationError
      ThenRules: ValidationRule<'a>[] }
    
let private createRule<'a> (condition: ValidationCondition<'a>) (error: ValidationError) =
    { Condition = condition
      Error = error
      ThenRules = [||] }
    
let private createWithThenRules<'a> (error: ValidationError) (condition: ValidationCondition<'a>) (thenRules: ValidationRule<'a>[]) =
    { Condition = condition
      Error = error
      ThenRules = thenRules }

let private isNotEmpty input = String.IsNullOrWhiteSpace(input) |> not
let private isRegex pattern input = Regex.IsMatch(input, pattern)
let private isLength (input: string) length = input.Length = length
let private isEqual input eqValue = input = eqValue

let private validate<'a> (rules: ValidationRule<'a>[]) (args: 'a) =
    let errors = ResizeArray<ValidationError>()
    let rec iter (rules: ValidationRule<'a>[]) (index: int) =
        if index <= rules.Length - 1 then
            if rules.[index].ThenRules.Length = 0 then
                if rules.[index].Condition args then iter rules (index + 1)
                else
                    errors.Add rules.[index].Error
                    iter rules (index + 1)
            else
                if rules.[index].Condition args then
                    iter rules.[index].ThenRules 0
                    iter rules (index + 1)
                else
                    errors.Add rules.[index].Error
                    iter rules (index + 1)
        else ()
    iter rules 0
    if (errors.Count = 0) then Ok args else errors |> Seq.toArray |> Error
    
let private phoneRegex = @"^\d{10}\b$"
let private passwordLength = 6
    
let private createUserRules =
    [|
        createWithThenRules<CreateUserArgs> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CreateUserArgs> (fun args -> isRegex phoneRegex args.Phone)
        |]
        
        ValidationError.NameIsEmpty |> createRule<CreateUserArgs> (fun args -> isNotEmpty args.Name)
        
        createWithThenRules<CreateUserArgs> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<CreateUserArgs> (fun args -> isLength args.Password passwordLength)
        |]
        
        ValidationError.ConfirmPasswordInvalid |> createRule<CreateUserArgs> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let private loginUserRules =
    [|
        createWithThenRules<LoginUserArgs> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<LoginUserArgs> (fun args -> isRegex phoneRegex args.Phone)
        |]
        createWithThenRules<LoginUserArgs> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<LoginUserArgs> (fun args -> isLength args.Password passwordLength)
        |]
    |]
    
let private codeRules =
    [|
        createWithThenRules<CodeArgs> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CodeArgs> (fun args -> isRegex phoneRegex args.Phone)
        |]
    |]
    
let private checkCodeRules =
    [|
        createWithThenRules<CheckCodeArgs> ValidationError.PhoneIsEmpty (fun args -> isNotEmpty args.Phone) [|
            ValidationError.PhoneRegexInvalid |> createRule<CheckCodeArgs> (fun args -> isRegex phoneRegex args.Phone)
        |]
        createWithThenRules<CheckCodeArgs> ValidationError.CodeIsEmpty (fun args -> args.Code.HasValue) [|
            ValidationError.CodeLengthInvalid |> createRule<CheckCodeArgs> (fun args -> args.Code.HasValue && args.Code.Value < 1000000 && args.Code.Value >= 100000)
        |]
    |]
    
let private changePasswordRules =
    [|
        ValidationError.IdIsEmpty |> createRule<ChangePasswordArgs> (fun args -> isNotEmpty args.Id)
        createWithThenRules<ChangePasswordArgs> ValidationError.PasswordIsEmpty (fun args -> isNotEmpty args.Password) [|
            ValidationError.PasswordLengthInvalid |> createRule<ChangePasswordArgs> (fun args -> isLength args.Password passwordLength)
        |]
        ValidationError.ConfirmPasswordInvalid |> createRule<ChangePasswordArgs> (fun args -> isEqual args.Password args.ConfirmPassword)
    |]
    
let validateCreateUserArgs (args: CreateUserArgs) = validate createUserRules args
let validateLoginUserArgs (args: LoginUserArgs) = validate loginUserRules args
let validateCodeArgs (args: CodeArgs) = validate codeRules args
let validateCheckCodeArgs (args: CheckCodeArgs) = validate checkCodeRules args
let validateChangePassword (args: ChangePasswordArgs) = validate changePasswordRules args
