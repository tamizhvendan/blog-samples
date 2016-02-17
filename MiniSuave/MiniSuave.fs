module MiniSuave
open Suave.Http
open Suave.Console
open Suave.Succuessful

[<EntryPoint>]
let main argv =
    let request = {Route = "/foo/bar"; Type = GET}
    let response = {Output = ""; Code = ""}
    let context = {Request = request; Response = response}
    execute context (OK "Hello Suave!")
    0
