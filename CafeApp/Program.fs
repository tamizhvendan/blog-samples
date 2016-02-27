module CafeApp
open ReadModel
open Data
open Suave.Web
open Api

[<EntryPoint>]
let main argv =
    
    startWebServer defaultConfig api
    0 // return an integer exit code