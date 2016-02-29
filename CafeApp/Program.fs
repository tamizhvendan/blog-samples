module CafeApp
open ReadModel
open Data
open Suave.Web
open Api
open EventsStore
[<EntryPoint>]
let main argv =

    startWebServer defaultConfig (api inMemoryEventStore)
    0 // return an integer exit code