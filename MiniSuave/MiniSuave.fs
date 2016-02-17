module MiniSuave
open Suave.Http
open Suave.Console
open Suave.Succuessful
open Suave.Combinators

[<EntryPoint>]
let main argv =
    let request = {Route = "/foo/bar"; Type = GET}
    let response = {Output = ""; Code = ""}
    let context = {Request = request; Response = response}
    executeInLoop context (Empty >=> OK "Hello, World!")
    0
