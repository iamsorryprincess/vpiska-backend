module internal Vpiska.Domain.Validation

open System
open System.Text.RegularExpressions

type ValidationCondition<'model> = 'model -> bool

type ValidationRule<'model, 'error> =
    { Condition: ValidationCondition<'model>
      Error: 'error
      ThenRules: ValidationRule<'model, 'error>[] }

let isNotNull input = Object.ReferenceEquals(input, null) |> not
    
let isNotEmpty input = String.IsNullOrWhiteSpace(input) |> not

let isRegex pattern input = Regex.IsMatch(input, pattern)

let isGreaterThan (input: string) length = input.Length >= length

let isEqual input eqValue = input = eqValue
    
let createRule<'model, 'error> (condition: ValidationCondition<'model>) (error: 'error) =
    { Condition = condition
      Error = error
      ThenRules = [||] }
    
let createWithThenRules<'model, 'error> (error: 'error) (condition: ValidationCondition<'model>) (thenRules: ValidationRule<'model, 'error>[]) =
    { Condition = condition
      Error = error
      ThenRules = thenRules }
    
let validate<'model, 'error> (rules: ValidationRule<'model, 'error>[]) (args: 'model) =
    let errors = ResizeArray<'error>()
    let rec iter (rules: ValidationRule<'model, 'error>[]) (index: int) =
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
    

