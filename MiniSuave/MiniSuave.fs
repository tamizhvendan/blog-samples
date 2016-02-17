module MiniSuave
open Suave.Http
open Suave.Console
open Suave.Succuessful
open Suave.Combinators
open Suave.Filters

[<EntryPoint>]
let main argv =
    let request = {Route = "/foo/bar"; Type = Suave.Http.GET}
    let response = {Output = ""; Code = ""}
    let context = {Request = request; Response = response}
    executeInLoop context (POST >=> OK "Hello, World!")
    0
