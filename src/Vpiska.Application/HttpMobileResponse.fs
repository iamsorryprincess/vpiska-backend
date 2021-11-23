namespace Vpiska.Application

type ErrorResponse = { ErrorCode: string }

type ApiResponse<'a> =
    { IsSuccess: bool
      Result: 'a
      Errors: ErrorResponse[] }
 
type ApiResponse =
    { IsSuccess: bool
      Result: string
      Errors: ErrorResponse[] }

module internal HttpMobileResponse =
    
    let createValueResult<'a> (result: 'a): ApiResponse<'a> = { IsSuccess = true; Result = result; Errors = [||] }
    
    let createResult () = { IsSuccess = true; Result = null; Errors = [||] }