module CafeApp

open Data
open Suave.Web
open Api
open EventsStore
[<EntryPoint>]
let main argv =
    use subscription =
      inMemoryEventStore.EventStream.Subscribe projectReadModel
    startWebServer defaultConfig (api inMemoryEventStore)
    0 // return an integer exit code