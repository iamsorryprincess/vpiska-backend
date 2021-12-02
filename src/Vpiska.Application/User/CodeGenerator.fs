module Vpiska.Application.User.CodeGenerator

open System

let private random = Random()

let generate () = random.Next(111111, 777777)
